using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

public class Report
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ReportedByUserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TargetType { get; set; } = string.Empty;

    public int TargetId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public int? ResolvedByAdminId { get; set; }

    [MaxLength(500)]
    public string? ResolutionNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
