using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── NOTIFICATION ──────────────────────────────────────────────
/// <summary>
/// Persisted notifications for offline users.
/// SignalR delivers real-time; this table delivers on next login.
/// </summary>
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    /// <summary>job_accepted | job_done | new_message | approved</summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    /// <summary>Optional — which job triggered this notification</summary>
    public int? RelatedJobId { get; set; }

    /// <summary>For new_message notifications — which conversation to navigate to</summary>
    public int? ConversationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Job? RelatedJob { get; set; }
    public Conversation? Conversation { get; set; }
}