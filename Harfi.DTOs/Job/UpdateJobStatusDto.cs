namespace Harfi.DTOs.Job;

public class JobResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? CraftsmanId { get; set; }
    public string Status { get; set; } = string.Empty;         // open, in-progress, done, rejected
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime? PreferredDate { get; set; }
    public string? ProblemImageUrl { get; set; }
    public string? ProblemDescription { get; set; }
    public string? SolutionDescription { get; set; }           // filled by craftsman when done
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}