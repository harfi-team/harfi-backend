using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── MEDIA FILE ────────────────────────────────────────────────
/// <summary>
/// Tracks all uploaded files (profile photos, ID images, job photos).
/// FileUrl points to Cloudinary.
/// EntityType + EntityId create a polymorphic reference.
/// </summary>
public class MediaFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Cloudinary URL</summary>
    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>image/jpeg | image/png | application/pdf</summary>
    [Required]
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;

    /// <summary>user | craftsman | job — polymorphic reference</summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public int UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User Uploader { get; set; } = null!;
}