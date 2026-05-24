using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── CONVERSATION ──────────────────────────────────────────────
/// <summary>
/// Container for Customer ↔ Craftsman chat.
/// JobId links the conversation to a job (1:1).
/// </summary>
public class Conversation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int JobId { get; set; }
    public int CustomerId { get; set; }
    public int CraftsmanId { get; set; }

    /// <summary>Updated on every new message — used to sort Inbox</summary>
    public DateTime? LastMessageAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public Job Job { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Craftsman Craftsman { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}