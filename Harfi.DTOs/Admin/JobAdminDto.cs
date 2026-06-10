namespace Harfi.DTOs.Admin;

public class JobAdminDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? CraftsmanId { get; set; }
    public string? CraftsmanName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsDisputed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class JobDetailDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? CraftsmanId { get; set; }
    public string? CraftsmanName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime? PreferredDate { get; set; }
    public string? ProblemImageUrl { get; set; }
    public string? ProblemDescription { get; set; }
    public string? SolutionDescription { get; set; }
    public bool IsDisputed { get; set; }
    public DateTime? DisputeRaisedAt { get; set; }
    public DateTime? DisputeResolvedAt { get; set; }
    public string? DisputeResolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateJobStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
}

public class FlagDisputeRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ResolveDisputeRequest
{
    public string Resolution { get; set; } = string.Empty;
    public string FavoredParty { get; set; } = string.Empty;
}

public class ChatMetadataDto
{
    public int ConversationId { get; set; }
    public int JobId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CraftsmanName { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
}
