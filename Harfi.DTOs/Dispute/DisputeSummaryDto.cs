namespace Harfi.DTOs.Dispute;

public class DisputeSummaryDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RaisedByRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? FavoredParty { get; set; }
}
