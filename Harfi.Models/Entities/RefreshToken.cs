using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

// ── REFRESH TOKEN ─────────────────────────────────────────────
/// <summary>
/// Enables long-lived sessions — users stay logged in
/// without needing to re-authenticate every hour.
/// </summary>
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User User { get; set; } = null!;

    // ── Helper Properties ──────────────────────────────────────
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;
}
