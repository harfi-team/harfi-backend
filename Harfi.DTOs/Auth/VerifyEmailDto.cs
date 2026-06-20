using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

public class VerifyEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "الكود يجب أن يكون 6 أرقام")]
    public string Code { get; set; } = null!;
}