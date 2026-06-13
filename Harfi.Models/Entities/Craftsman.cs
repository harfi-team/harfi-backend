using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

public class Craftsman
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK → Users (1:1 relationship)</summary>
    public int UserId { get; set; }

    [Required]
    public int ServiceTypeId { get; set; }

    [Required]
    public int CityId { get; set; }

    [MaxLength(100)]
    public string? Neighborhood { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PriceRangeMin { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PriceRangeMax { get; set; }

    public int Experience { get; set; } = 0;

    /// <summary>Admin must approve before craftsman appears in search</summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>Craftsman sets this — available for new jobs or not</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Soft delete flag — never hard-delete craftsmen</summary>
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByAdminId { get; set; }

    [MaxLength(500)]
    public string? DeletionReason { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>Computed from Reviews — do NOT update manually</summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal? Rating { get; set; } = null;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? NationalIdUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────────────
    public User User { get; set; } = null!;
    public virtual ServiceType? Service { get; set; }
    public virtual City? CityNavigation { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
