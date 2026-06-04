using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── MESSAGE ───────────────────────────────────────────────────
/// <summary>Individual message inside a Conversation</summary>
public class Message
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public int SenderId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>text | image | system</summary>
    [MaxLength(20)]
    public string MessageType { get; set; } = "text";

    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
}