using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── RAG DOCUMENT ─────────────────────────────────────────────
/// <summary>
/// SQL reference for documents stored in ChromaDB.
/// Allows tracking, re-embedding, and cleanup of RAG knowledge base.
/// </summary>
public class RAGDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int JobId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ChromaDocumentId { get; set; } = string.Empty;

    /// <summary>problem | solution</summary>
    [Required]
    [MaxLength(20)]
    public string ChunkType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public Job Job { get; set; } = null!;
    public ICollection<JobFeedback> Feedbacks { get; set; } = new List<JobFeedback>();
}