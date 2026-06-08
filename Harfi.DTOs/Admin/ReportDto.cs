namespace Harfi.DTOs.Admin;

public class ReportDto
{
    public int Id { get; set; }
    public int ReportedByUserId { get; set; }
    public string ReportedByUserName { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? ResolvedByAdminId { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ResolveReportRequest
{
    public string Action { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
