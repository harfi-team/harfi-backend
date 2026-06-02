using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

public class SendPhoneVerificationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;
}
