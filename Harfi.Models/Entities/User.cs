using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Harfi.Models.Entities;

public class User : IdentityUser<int>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>admin | craftsman | customer</summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>Soft delete flag — never hard-delete users</summary>
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByAdminId { get; set; }
    public bool IsVerified { get; set; } = false;

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(500)]
    public string? DeletionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public Craftsman? CraftsmanProfile { get; set; }
    public ICollection<Job> JobsAsCustomer { get; set; } = new List<Job>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AIChatMessage> AIChatMessages { get; set; } = new List<AIChatMessage>();
    public ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<MediaFile> UploadedFiles { get; set; } = new List<MediaFile>();
    public ICollection<EmailVerification> EmailVerifications { get; set; }
        = new List<EmailVerification>();
}
