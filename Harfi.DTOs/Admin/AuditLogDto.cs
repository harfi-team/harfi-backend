namespace Harfi.DTOs.Admin;

public class AuditLogDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string? Notes { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
