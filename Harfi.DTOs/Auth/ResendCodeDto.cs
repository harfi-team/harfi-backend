using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

public class ResendCodeDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}