using Harfi.DTOs.Auth;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly IGenericRepository<PhoneVerification> _phoneVerificationRepo;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _config;
    private readonly ICraftsmanRepository _craftsmanRepo;
    private readonly ILogger<AuthService> _logger;

    // ── CONSTRUCTOR ───────────────────────────────────────────
    public AuthService(
        UserManager<User> userManager,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IGenericRepository<EmailVerification> verificationRepo,
        IGenericRepository<PhoneVerification> phoneVerificationRepo,
        IEmailService emailService,
        ISmsService smsService,
        IConfiguration config,
        ICraftsmanRepository craftsmanRepo,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _refreshTokenRepo = refreshTokenRepo;
        _verificationRepo = verificationRepo;
        _phoneVerificationRepo = phoneVerificationRepo;
        _emailService = emailService;
        _smsService = smsService;
        _config = config;
        _craftsmanRepo = craftsmanRepo;
        _logger = logger;
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

        // 4. Generate Identity email confirmation token + custom 6-digit code
        var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = GenerateSecureOtp();
        await _verificationRepo.AddAsync(new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            IdentityToken = identityToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });
        await _verificationRepo.SaveChangesAsync();

        // 5. Send verification email (non-blocking — network failure must not crash registration)
        try
        {
            await _emailService.SendVerificationCodeAsync(user.Email!, user.Name, code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send verification email to {Email}. Registration completed anyway.",
                user.Email);
        }

        // 6. If user provided a phone number, auto-initiate phone verification
        bool requiresPhoneVerification = false;
        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            try
            {
                var phoneIdentityToken = await _userManager.GenerateChangePhoneNumberTokenAsync(
                    user, dto.Phone);
                var phoneCode = GenerateSecureOtp();
                await _phoneVerificationRepo.AddAsync(new PhoneVerification
                {
                    UserId = user.Id,
                    PhoneNumber = dto.Phone,
                    Code = phoneCode,
                    IdentityToken = phoneIdentityToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false
                });
                await _phoneVerificationRepo.SaveChangesAsync();

                var sent = await _smsService.SendOtpAsync(dto.Phone, phoneCode);
                if (sent)
                    requiresPhoneVerification = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send phone verification for user {Email}. Registration completed anyway.",
                    user.Email);
            }
        }

        // 7. Return tokens
        var response = await BuildAuthResponseAsync(user);
        response.RequiresPhoneVerification = requiresPhoneVerification;
        return response;
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
                "الحساب غير مفعّل. تواصل مع الدعم.");

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
        // 1. Find token (hash incoming token for comparison)
        var stored = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == HashToken(refreshToken));

        // 2. Validate
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException(
                "رمز التحديث غير صالح أو منتهي الصلاحية.");

        // 3. Revoke old token
        stored.IsRevoked = true;
        _refreshTokenRepo.Update(stored);
        await _refreshTokenRepo.SaveChangesAsync();

        // 4. Load user
        var user = await _userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new UnauthorizedAccessException("المستخدم غير موجود.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("الحساب غير مفعّل. تواصل مع الدعم.");

        // 5. Return new tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── LOGOUT ────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == HashToken(refreshToken));

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

        // Mark code as used
        verification.IsUsed = true;
        _verificationRepo.Update(verification);

        // Use Identity's ConfirmEmailAsync — this validates the token
        // and automatically flips the EmailConfirmed column to true.
        var confirmResult = await _userManager.ConfirmEmailAsync(user, verification.IdentityToken!);

        if (!confirmResult.Succeeded)
        {
            var errors = string.Join("; ", confirmResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل تأكيد البريد الإلكتروني: {errors}");
        }

        // Preserve custom business logic: mark user as verified
        user.IsVerified = true;
        await _userManager.UpdateAsync(user);

        return "تم تفعيل البريد الإلكتروني بنجاح.";
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

        // Generate new Identity token + custom code
        var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = GenerateSecureOtp();
        await _verificationRepo.AddAsync(new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            IdentityToken = identityToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });

        await _verificationRepo.SaveChangesAsync();
        await _emailService.SendVerificationCodeAsync(user.Email!, user.Name, code);

        return "تم إعادة إرسال الكود بنجاح.";
    }

    // ── SEND PHONE VERIFICATION CODE ─────────────────────────
    public async Task<string> SendPhoneVerificationCodeAsync(string email, string phoneNumber)
    {
        var user = await _userManager.FindByEmailAsync(email.ToLower().Trim())
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.PhoneNumberConfirmed)
            throw new InvalidOperationException("رقم الهاتف مفعّل مسبقاً.");

        var identityToken = await _userManager.GenerateChangePhoneNumberTokenAsync(
            user, phoneNumber);
        var code = GenerateSecureOtp();

        await _phoneVerificationRepo.AddAsync(new PhoneVerification
        {
            UserId = user.Id,
            PhoneNumber = phoneNumber,
            Code = code,
            IdentityToken = identityToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });
        await _phoneVerificationRepo.SaveChangesAsync();

        var sent = await _smsService.SendOtpAsync(phoneNumber, code);
        if (!sent)
            throw new InvalidOperationException("فشل إرسال رسالة التحقق، حاول مرة أخرى");

        return "تم إرسال الكود بنجاح.";
    }

    // ── VERIFY PHONE ──────────────────────────────────────────
    public async Task<string> VerifyPhoneAsync(string email, string phoneNumber, string code)
    {
        var user = await _userManager.FindByEmailAsync(email.ToLower().Trim())
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.PhoneNumberConfirmed)
            throw new InvalidOperationException("رقم الهاتف مفعّل مسبقاً.");

        var verification = await _phoneVerificationRepo.FirstOrDefaultAsync(
            v => v.UserId == user.Id &&
                 v.PhoneNumber == phoneNumber &&
                 v.Code == code &&
                 !v.IsUsed &&
                 v.ExpiresAt > DateTime.UtcNow);

        if (verification is null)
            throw new InvalidOperationException("الكود غير صحيح أو منتهي الصلاحية.");

        verification.IsUsed = true;
        _phoneVerificationRepo.Update(verification);

        var confirmResult = await _userManager.ChangePhoneNumberAsync(
            user, verification.PhoneNumber, verification.IdentityToken!);

        if (!confirmResult.Succeeded)
        {
            var errors = string.Join("; ", confirmResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل تأكيد رقم الهاتف: {errors}");
        }

        user.Phone = verification.PhoneNumber;
        await _userManager.UpdateAsync(user);

        return "تم تفعيل رقم الهاتف بنجاح.";
    }

    // ── RESEND PHONE CODE ─────────────────────────────────────
    public async Task<string> ResendPhoneVerificationCodeAsync(string email, string phoneNumber)
    {
        var user = await _userManager.FindByEmailAsync(email.ToLower().Trim())
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.PhoneNumberConfirmed)
            throw new InvalidOperationException("رقم الهاتف مفعّل مسبقاً.");

        var oldCodes = await _phoneVerificationRepo.FindAsync(
            v => v.UserId == user.Id && v.PhoneNumber == phoneNumber && !v.IsUsed);

        foreach (var old in oldCodes)
        {
            old.IsUsed = true;
            _phoneVerificationRepo.Update(old);
        }

        var identityToken = await _userManager.GenerateChangePhoneNumberTokenAsync(
            user, phoneNumber);
        var code = GenerateSecureOtp();

        await _phoneVerificationRepo.AddAsync(new PhoneVerification
        {
            UserId = user.Id,
            PhoneNumber = phoneNumber,
            Code = code,
            IdentityToken = identityToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });

        await _phoneVerificationRepo.SaveChangesAsync();

        var sent = await _smsService.SendOtpAsync(phoneNumber, code);
        if (!sent)
            throw new InvalidOperationException("فشل إرسال رسالة التحقق، حاول مرة أخرى");

        return "تم إعادة إرسال الكود بنجاح.";
    }

    // ── GET CRAFTSMAN PROFILE BY USER ID ─────────────────────
    public async Task<Craftsman?> GetCraftsmanProfileByUserIdAsync(int userId)
    {
        return await _craftsmanRepo.GetByUserIdAsync(userId);
    }

    // ── FORGOT PASSWORD ────────────────────────────────────────
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.ToLower().Trim());

        // Silently return if email not found (don't reveal whether email exists)
        if (user is null) return;

        var code = GenerateSecureOtp();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.Name, code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
        }
    }

    // ── RESET PASSWORD ─────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.ToLower().Trim())
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.PasswordResetCode is null || user.PasswordResetCodeExpiry is null)
            throw new InvalidOperationException("لم يتم طلب إعادة تعيين كلمة المرور.");

        if (user.PasswordResetCode != dto.Code)
            throw new InvalidOperationException("الكود غير صحيح.");

        if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("الكود منتهي الصلاحية.");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل إعادة تعيين كلمة المرور: {errors}");
        }

        var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل إعادة تعيين كلمة المرور: {errors}");
        }

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    // ══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ══════════════════════════════════════════════════════════

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var expiryMinutes = GetJwtSetting<int>("AccessTokenExpiryMinutes", 60);
        var accessToken = GenerateJwtToken(user);
        var newRefresh = await CreateAndSaveRefreshTokenAsync(user.Id);

        var craftsman = user.Role == "craftsman"
            ? await _craftsmanRepo.GetByUserIdAsync(user.Id)
            : null;

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
                ProfileImageUrl = user.ProfileImageUrl,
                CraftsmanId = craftsman?.Id
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("مفتاح JWT السري غير مضبوط.");

        if (string.IsNullOrEmpty(user.Email))
            throw new InvalidOperationException("البريد الإلكتروني للمستخدم مفقود.");

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

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedToken = HashToken(rawToken);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(refreshToken);
        await _refreshTokenRepo.SaveChangesAsync();

        // Return object with raw token for HTTP response (client receives raw token)
        return new RefreshToken 
        { 
            Id = refreshToken.Id,
            UserId = userId,
            Token = rawToken,  // Return raw token to client
            ExpiresAt = refreshToken.ExpiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private T GetJwtSetting<T>(string key, T defaultValue)
    {
        var raw = _config[$"JwtSettings:{key}"];
        if (raw is null) return defaultValue;
        try { return (T)Convert.ChangeType(raw, typeof(T)); }
        catch { return defaultValue; }
    }

    private static string GenerateSecureOtp()
    {
        byte[] data = new byte[4];
        System.Security.Cryptography.RandomNumberGenerator.Fill(data);
        uint value = BitConverter.ToUInt32(data, 0) % 900000;
        return (100000 + value).ToString();
    }

    private static string HashToken(string token)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
