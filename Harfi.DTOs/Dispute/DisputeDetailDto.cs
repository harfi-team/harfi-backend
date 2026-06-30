namespace Harfi.DTOs.Dispute;

public class DisputeDetailDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobServiceType { get; set; } = string.Empty;
    public string JobStatus { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;

    // Who raised it
    public int RaisedByUserId { get; set; }
    public string RaisedByUserName { get; set; } = string.Empty;
    public string RaisedByRole { get; set; } = string.Empty;

    // Dispute details
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Attachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // The other party's response
    public string? ResponseMessage { get; set; }
    public string? ResponseAttachments { get; set; }

    // Resolution
    public string? Resolution { get; set; }
    public string? FavoredParty { get; set; }
    public int? ResolvedByAdminId { get; set; }
    public string? ResolvedByAdminName { get; set; }

    // Parties info
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? CraftsmanId { get; set; }
    public string? CraftsmanName { get; set; }

    // Chat reference for investigation
    public int? ConversationId { get; set; }

    // Other active disputes on the same job
    public bool HasActiveDispute { get; set; }
}
