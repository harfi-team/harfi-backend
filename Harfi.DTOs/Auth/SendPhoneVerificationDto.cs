using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

public class SendPhoneVerificationDto
{
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;
}
