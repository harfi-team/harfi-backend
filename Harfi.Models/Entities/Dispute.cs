using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Harfi.Models.Constants;

namespace Harfi.Models.Entities;

public class Dispute
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int JobId { get; set; }

    /// <summary>Who raised the dispute — Customer or Craftsman user ID</summary>
    public int RaisedByUserId { get; set; }

    /// <summary>"customer" | "craftsman"</summary>
    [Required]
    [MaxLength(20)]
    public string RaisedByRole { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>JSON array of image URLs, e.g. ["url1","url2"]</summary>
    [MaxLength(2000)]
    public string? Attachments { get; set; }

    /// <summary>قيد المراجعة | قيد التحقيق | تم الحل | مرفوض</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = DisputeStatusConstants.Pending;

    [MaxLength(2000)]
    public string? Resolution { get; set; }

    [MaxLength(20)]
    public string? FavoredParty { get; set; }

    public int? ResolvedByAdminId { get; set; }

    /// <summary>The other party's response/evidence submitted before admin resolution</summary>
    [MaxLength(2000)]
    public string? ResponseMessage { get; set; }

    /// <summary>Attachments submitted by the responding party</summary>
    [MaxLength(2000)]
    public string? ResponseAttachments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    // ── Navigation Properties ──────────────────────────────────
    public Job Job { get; set; } = null!;
    public User RaisedByUser { get; set; } = null!;
    public User? ResolvedByAdmin { get; set; }
}
