namespace Harfi.DTOs.Job;

public class CreateJobDto
{
    public int CraftsmanId { get; set; }          // which craftsman they want
    public string ServiceType { get; set; } = string.Empty;   // e.g. "plumber", "electrician"
    public string Description { get; set; } = string.Empty;   // what they need done
    public string Address { get; set; } = string.Empty;        // where the job is
    public DateTime? PreferredDate { get; set; }               // when they want it
    public string? ProblemImageUrl { get; set; }               // optional photo
    public string? ProblemDescription { get; set; }            // optional extra details
}