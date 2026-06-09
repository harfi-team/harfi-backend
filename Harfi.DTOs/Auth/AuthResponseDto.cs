using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

// ── USER INFO (embedded in AuthResponse) ─────────────────────
public class UserInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
}

// ── AUTH RESPONSE ─────────────────────────────────────────────
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
    public bool RequiresPhoneVerification { get; set; }
}