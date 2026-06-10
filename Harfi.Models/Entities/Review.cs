using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── REVIEW ────────────────────────────────────────────────────
/// <summary>
/// One review per job (UNIQUE on JobId).
/// Only allowed when Job.Status == "مكتمل"
/// </summary>
public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int JobId { get; set; }
    public int CustomerId { get; set; }
    public int CraftsmanId { get; set; }

    /// <summary>1 to 5 — enforced in service layer</summary>
    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByAdminId { get; set; }

    [MaxLength(500)]
    public string? DeletionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public Job Job { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Craftsman Craftsman { get; set; } = null!;
}