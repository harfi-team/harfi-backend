using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

public class VerifyPhoneDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; } = null!;
}
