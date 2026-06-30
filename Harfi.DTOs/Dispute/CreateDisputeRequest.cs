using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Dispute;

public class CreateDisputeRequest
{
    [Required(ErrorMessage = "سبب النزاع مطلوب")]
    [MinLength(10, ErrorMessage = "يجب أن يكون سبب النزاع 10 أحرف على الأقل")]
    [MaxLength(500, ErrorMessage = "سبب النزاع يجب ألا يتجاوز 500 حرف")]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "الوصف يجب ألا يتجاوز 2000 حرف")]
    public string? Description { get; set; }

    /// <summary>JSON array of image URLs</summary>
    [MaxLength(2000)]
    public string? Attachments { get; set; }
}
