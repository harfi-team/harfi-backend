using Harfi.DTOs.Auth;
using Harfi.Models.Entities;

namespace Harfi.Services.Interfaces;

/// <summary>
/// Handles all authentication operations.
/// Controllers call this — never touch DB directly.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user (customer or craftsman).
    /// Throws InvalidOperationException if email already exists.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Login with email + password.
    /// Throws UnauthorizedAccessException if credentials are wrong or account is inactive.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    /// <summary>
    /// Exchange a valid refresh token for a new access token + new refresh token.
    /// Throws UnauthorizedAccessException if token is invalid, revoked, or expired.
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoke a refresh token (logout).
    /// Silent if token not found.
    /// </summary>
    Task LogoutAsync(string refreshToken);

    Task<string> VerifyEmailAsync(VerifyEmailDto dto);
    Task<string> ResendVerificationCodeAsync(ResendCodeDto dto);

    Task<string> SendPhoneVerificationCodeAsync(string email, string phoneNumber);
    Task<string> VerifyPhoneAsync(string email, string phoneNumber, string code);
    Task<string> ResendPhoneVerificationCodeAsync(string email, string phoneNumber);
    Task<Craftsman?> GetCraftsmanProfileByUserIdAsync(int userId);

    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
