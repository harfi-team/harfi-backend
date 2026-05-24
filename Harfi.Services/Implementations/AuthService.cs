using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Harfi.DTOs.Auth;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Harfi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IConfiguration _config;

    public AuthService(
        IGenericRepository<User> userRepo,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _config = config;
    }

    // ── REGISTER ─────────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1. Check email is unique (case-insensitive)
        var emailTaken = await _userRepo.ExistsAsync(
            u => u.Email == dto.Email.ToLower().Trim());

        if (emailTaken)
            throw new InvalidOperationException(
                "البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول.");

        // 2. Create user entity
        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Phone = dto.Phone?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save to DB
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // 4. Return tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── LOGIN ─────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // 1. Find user by email
        var user = await _userRepo.FirstOrDefaultAsync(
            u => u.Email == dto.Email.ToLower().Trim());

        // 2. Verify password — same error message for security (no email enumeration)
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException(
                "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        // 3. Check account is active
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "هذا الحساب موقوف. تواصل مع الدعم الفني.");

        // 4. Return tokens
        return await BuildAuthResponseAsync(user);
    }

    // ── REFRESH TOKEN ─────────────────────────────────────────
    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // 1. Find the token in DB
        var stored = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken);

        // 2. Validate
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException(
                "رمز التحديث غير صالح أو منتهي الصلاحية. سجّل الدخول مرة أخرى.");

        // 3. Revoke the old token (rotation — one-time use)
        stored.IsRevoked = true;
        _refreshTokenRepo.Update(stored);
        await _refreshTokenRepo.SaveChangesAsync();

        // 4. Load user
        var user = await _userRepo.GetByIdAsync(stored.UserId)
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

    // ══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Builds the full AuthResponseDto with a new JWT + refresh token.
    /// Called by Register, Login, and RefreshToken.
    /// </summary>
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
                Email = user.Email,
                Role = user.Role,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl
            }
        };
    }

    /// <summary>Creates and signs a JWT token for the given user.</summary>
    private string GenerateJwtToken(User user)
    {
        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email),
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

    /// <summary>
    /// Generates a cryptographically secure refresh token,
    /// saves it to DB, and returns the entity.
    /// </summary>
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

    /// <summary>Safely reads a typed value from JwtSettings config.</summary>
    private T GetJwtSetting<T>(string key, T defaultValue)
    {
        var raw = _config[$"JwtSettings:{key}"];
        if (raw is null) return defaultValue;

        try { return (T)Convert.ChangeType(raw, typeof(T)); }
        catch { return defaultValue; }
    }
}
