using Harfi.DTOs.Auth;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Harfi.Services.Implementations;

public class AuthService : IAuthService
{
    // ── FIELDS ────────────────────────────────────────────────
    private readonly UserManager<User> _userManager;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IGenericRepository<EmailVerification> _verificationRepo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    // ── CONSTRUCTOR ───────────────────────────────────────────
    public AuthService(
        UserManager<User> userManager,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IGenericRepository<EmailVerification> verificationRepo,
        IEmailService emailService,
        IConfiguration config)
    {
        _userManager = userManager;
        _refreshTokenRepo = refreshTokenRepo;
        _verificationRepo = verificationRepo;
        _emailService = emailService;
        _config = config;
    }

    // ── REGISTER ──────────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1. Check email is unique
        var existingUser = await _userManager.FindByEmailAsync(dto.Email.ToLower().Trim());

        if (existingUser is not null)
            throw new InvalidOperationException(
                "البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول.");

        // 2. Create user
        var user = new User
        {
            UserName = dto.Email.ToLower().Trim(),
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLower().Trim(),
            Role = dto.Role,
            Phone = dto.Phone?.Trim(),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save user with Identity
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        // 4. Generate & save verification code
        var code = new Random().Next(100000, 999999).ToString();
        await _verificationRepo.AddAsync(new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });
        await _verificationRepo.SaveChangesAsync();

        // 5. Send verification email
        await _emailService.SendVerificationCodeAsync(user.Email!, user.Name, code);

        // 6. Return tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── LOGIN ─────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // 1. Find user
        var user = await _userManager.FindByEmailAsync(dto.Email.ToLower().Trim());

        // 2. Verify password
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException(
                "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        // 3. Check active
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "هذا الحساب موقوف. تواصل مع الدعم الفني.");

        // 4. Check verified
        if (!user.IsVerified)
            throw new UnauthorizedAccessException(
                "البريد الإلكتروني غير مفعّل. تحقق من بريدك الإلكتروني.");

        // 5. Return tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── REFRESH TOKEN ─────────────────────────────────────────
    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // 1. Find token
        var stored = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken);

        // 2. Validate
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException(
                "رمز التحديث غير صالح أو منتهي الصلاحية. سجّل الدخول مرة أخرى.");

        // 3. Revoke old token
        stored.IsRevoked = true;
        _refreshTokenRepo.Update(stored);
        await _refreshTokenRepo.SaveChangesAsync();

        // 4. Load user
        var user = await _userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new UnauthorizedAccessException("المستخدم غير موجود.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("هذا الحساب موقوف.");

        // 5. Return new tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── LOGOUT ────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken);

        if (stored is not null && !stored.IsRevoked)
        {
            stored.IsRevoked = true;
            _refreshTokenRepo.Update(stored);
            await _refreshTokenRepo.SaveChangesAsync();
        }
        // Silent if not found — logout should never throw
    }

    // ── VERIFY EMAIL ──────────────────────────────────────────
    public async Task<string> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.ToLower());

        if (user is null)
            throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.IsVerified)
            throw new InvalidOperationException("البريد الإلكتروني مفعّل مسبقاً.");

        var verification = await _verificationRepo.FirstOrDefaultAsync(
            v => v.UserId == user.Id &&
                 v.Code == dto.Code &&
                 !v.IsUsed &&
                 v.ExpiresAt > DateTime.UtcNow);

        if (verification is null)
            throw new InvalidOperationException("الكود غير صحيح أو منتهي الصلاحية.");

        // Mark as verified
        verification.IsUsed = true;
        user.IsVerified = true;

        _verificationRepo.Update(verification);
        await _userManager.UpdateAsync(user); // saves both user + verification

        return "تم تفعيل البريد الإلكتروني بنجاح. يمكنك تسجيل الدخول الآن.";
    }

    // ── RESEND CODE ───────────────────────────────────────────
    public async Task<string> ResendVerificationCodeAsync(ResendCodeDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.ToLower());

        if (user is null)
            throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.IsVerified)
            throw new InvalidOperationException("البريد الإلكتروني مفعّل مسبقاً.");

        // Invalidate all old unused codes
        var oldCodes = await _verificationRepo.FindAsync(
            v => v.UserId == user.Id && !v.IsUsed);

        foreach (var old in oldCodes)
        {
            old.IsUsed = true;
            _verificationRepo.Update(old);
        }

        // Generate new code
        var code = new Random().Next(100000, 999999).ToString();
        await _verificationRepo.AddAsync(new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });

        await _verificationRepo.SaveChangesAsync();
        await _emailService.SendVerificationCodeAsync(user.Email!, user.Name, code);

        return "تم إرسال كود جديد إلى بريدك الإلكتروني.";
    }

    // ══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ══════════════════════════════════════════════════════════

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var expiryMinutes = GetJwtSetting<int>("AccessTokenExpiryMinutes", 60);
        var accessToken = GenerateJwtToken(user);
        var newRefresh = await CreateAndSaveRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefresh.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email!,
                Role = user.Role,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        if (string.IsNullOrEmpty(user.Email))
            throw new InvalidOperationException("User email is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email!),
            new(ClaimTypes.Role,           user.Role),
            new(ClaimTypes.Name,           user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var expiryMinutes = GetJwtSetting<int>("AccessTokenExpiryMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> CreateAndSaveRefreshTokenAsync(int userId)
    {
        var expiryDays = GetJwtSetting<int>("RefreshTokenExpiryDays", 30);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(refreshToken);
        await _refreshTokenRepo.SaveChangesAsync();

        return refreshToken;
    }

    private T GetJwtSetting<T>(string key, T defaultValue)
    {
        var raw = _config[$"JwtSettings:{key}"];
        if (raw is null) return defaultValue;
        try { return (T)Convert.ChangeType(raw, typeof(T)); }
        catch { return defaultValue; }
    }
}
