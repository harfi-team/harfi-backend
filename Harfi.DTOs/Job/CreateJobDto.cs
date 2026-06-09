using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Job;

public class CreateJobDto
{
    public int? CraftsmanId { get; set; }

    [Required(ErrorMessage = "نوع الخدمة مطلوب")]
    [MaxLength(50)]
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف المشكلة مطلوب")]
    [MinLength(10, ErrorMessage = "يجب أن يكون الوصف 10 أحرف على الأقل")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public DateTime? PreferredDate { get; set; }

    [MaxLength(500)]
    public string? ProblemImageUrl { get; set; }

    [MaxLength(2000)]
    public string? ProblemDescription { get; set; }
}