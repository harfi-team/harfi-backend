using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── JOB FEEDBACK ──────────────────────────────────────────────
/// <summary>
/// User feedback on AI-generated guidance.
/// Used to improve RAG knowledge base quality over time.
/// </summary>
public class JobFeedback
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int? RAGDocumentId { get; set; }

    /// <summary>helpful | not_helpful</summary>
    [Required]
    [MaxLength(20)]
    public string FeedbackType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User User { get; set; } = null!;
    public RAGDocument? RAGDocument { get; set; }
}