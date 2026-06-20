using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Harfi.Models.Constants;

namespace Harfi.Models.Entities;

public class Job
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    /// <summary>Nullable — assigned after craftsman accepts</summary>
    public int? CraftsmanId { get; set; }

    /// <summary>مفتوح | قيد التنفيذ | مكتمل | مرفوض | ملغى</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = JobStatusConstants.Open;

    [Required]
    [MaxLength(50)]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public DateTime? PreferredDate { get; set; }

    /// <summary>Used by GPT-4o Vision for problem analysis</summary>
    [MaxLength(500)]
    public string? ProblemImageUrl { get; set; }

    /// <summary>Summary after job completion — stored in ChromaDB too</summary>
    [MaxLength(2000)]
    public string? ProblemDescription { get; set; }

    /// <summary>Solution summary — goes to ChromaDB for RAG</summary>
    [MaxLength(2000)]
    public string? SolutionDescription { get; set; }

    /// <summary>Dispute management fields</summary>
    public bool IsDisputed { get; set; } = false;
    public DateTime? DisputeRaisedAt { get; set; }
    public DateTime? DisputeResolvedAt { get; set; }

    [MaxLength(500)]
    public string? DisputeResolution { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User Customer { get; set; } = null!;
    public Craftsman? Craftsman { get; set; }
    public Review? Review { get; set; }
    public Conversation? Conversation { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RAGDocument> RAGDocuments { get; set; } = new List<RAGDocument>();
    public ICollection<JobFeedback> Feedbacks { get; set; } = new List<JobFeedback>();
}
