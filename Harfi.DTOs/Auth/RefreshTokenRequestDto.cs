using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;



// ── REFRESH TOKEN REQUEST ─────────────────────────────────────

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "رمز التحديث مطلوب")]
    public string RefreshToken { get; set; } = string.Empty;
}