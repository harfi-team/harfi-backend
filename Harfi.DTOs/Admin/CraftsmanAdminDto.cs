namespace Harfi.DTOs.Admin;

public class PendingCraftsmanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public int Experience { get; set; }
    public string? NationalIdUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovedCraftsmanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public int Experience { get; set; }
    public decimal Rating { get; set; }
    public bool IsAvailable { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RejectedCraftsmanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class CraftsmanDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public decimal? PriceRangeMin { get; set; }
    public decimal? PriceRangeMax { get; set; }
    public int Experience { get; set; }
    public bool IsApproved { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDeleted { get; set; }
    public decimal Rating { get; set; }
    public string? Bio { get; set; }
    public string? NationalIdUrl { get; set; }
    public string? RejectionReason { get; set; }
    public string? DeletionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CompletedJobsCount { get; set; }
    public int TotalReviews { get; set; }
}

public class RejectCraftsmanRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class SuspendCraftsmanRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class DeleteCraftsmanRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ApproveCraftsmanRequest
{
    public string? NotifyMessage { get; set; }
}
