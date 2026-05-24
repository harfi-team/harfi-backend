//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Harfi.Models.Entities;

//public class User
//{
//    [Key]
//    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public string Name { get; set; } = string.Empty;

//    [Required]
//    [MaxLength(200)]
//    [EmailAddress]
//    public string Email { get; set; } = string.Empty;

//    /// <summary>BCrypt hashed password — never store plain text</summary>
//    [Required]
//    [MaxLength(500)]
//    public string PasswordHash { get; set; } = string.Empty;

//    /// <summary>admin | craftsman | customer</summary>
//    [Required]
//    [MaxLength(20)]
//    public string Role { get; set; } = string.Empty;

//    [MaxLength(20)]
//    public string? Phone { get; set; }

//    /// <summary>Soft delete flag — never hard-delete users</summary>
//    public bool IsActive { get; set; } = true;

//    [MaxLength(500)]
//    public string? ProfileImageUrl { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//    // ── Navigation Properties ──────────────────────────────────
//    public Craftsman? CraftsmanProfile { get; set; }
//    public ICollection<Job> JobsAsCustomer { get; set; } = new List<Job>();
//    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
//    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
//    public ICollection<AIChatMessage> AIChatMessages { get; set; } = new List<AIChatMessage>();
//    public ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
//    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
//    public ICollection<MediaFile> UploadedFiles { get; set; } = new List<MediaFile>();
//}
