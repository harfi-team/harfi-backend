using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── USER CONNECTION ───────────────────────────────────────────
/// <summary>
/// Tracks SignalR Hub connections per user.
/// One user can have multiple connections (phone + laptop).
/// ConnectionId is temporary and assigned by SignalR Hub.
/// </summary>
public class UserConnection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>Assigned by SignalR Hub.OnConnectedAsync()</summary>
    [Required]
    [MaxLength(100)]
    public string ConnectionId { get; set; } = string.Empty;

    public bool IsConnected { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User User { get; set; } = null!;
}
