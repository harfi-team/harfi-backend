using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Auth;

// ── REGISTER ─────────────────────────────────────────────────
public class RegisterDto
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يزيد عن 100 حرف")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>customer | craftsman — admin يتعمل manually فقط</summary>
    [Required(ErrorMessage = "الدور مطلوب")]
    [RegularExpression("^(customer|craftsman)$",
        ErrorMessage = "الدور يجب أن يكون customer أو craftsman")]
    public string Role { get; set; } = "customer";

    [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
    [MaxLength(20)]
    public string? Phone { get; set; }
}