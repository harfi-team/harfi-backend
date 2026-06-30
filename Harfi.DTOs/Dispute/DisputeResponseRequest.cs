using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Dispute;

public class DisputeResponseRequest
{
    [Required(ErrorMessage = "الرد على النزاع مطلوب")]
    [MinLength(10, ErrorMessage = "يجب أن يكون الرد 10 أحرف على الأقل")]
    [MaxLength(2000, ErrorMessage = "الرد يجب ألا يتجاوز 2000 حرف")]
    public string Message { get; set; } = string.Empty;

    /// <summary>JSON array of image URLs</summary>
    [MaxLength(2000)]
    public string? Attachments { get; set; }
}
