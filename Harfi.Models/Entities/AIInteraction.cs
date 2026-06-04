using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── AI CHAT MESSAGE ───────────────────────────────────────────
/// <summary>
/// Stores every message in AI Agent conversations (Semantic Kernel history).
/// Each row = one message. SessionId groups messages of same conversation.
/// user | assistant roles per row.
/// </summary>
public class AIChatMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>GUID grouping messages of same conversation session</summary>
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>user | assistant</summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>CraftsmanSearchTool | SelfFixGuideTool | PricingEstimatorTool</summary>
    [MaxLength(100)]
    public string? ToolUsed { get; set; }

    public int? TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User User { get; set; } = null!;
}
