namespace Harfi.DTOs.Admin;

public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalCraftsmen { get; set; }
    public int PendingCraftsmen { get; set; }
    public int ActiveJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int DisputedJobs { get; set; }
    public int PendingReports { get; set; }
    public int TotalReviews { get; set; }
    public int NewUsersThisMonth { get; set; }
    public double AverageRating { get; set; }
    public List<RecentCraftsmanDto> RecentCraftsmen { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class RecentCraftsmanDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecentOrderDto
{
    public int Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CraftsmanAnalyticsDto
{
    public int TotalCraftsmen { get; set; }
    public int PendingApproval { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Suspended { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<string, int> ByServiceType { get; set; } = new();
    public Dictionary<string, int> ByCity { get; set; } = new();
}

public class JobAnalyticsDto
{
    public int TotalJobs { get; set; }
    public int Open { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Rejected { get; set; }
    public int Disputed { get; set; }
    public Dictionary<string, int> ByServiceType { get; set; } = new();
    public double AverageCompletionDays { get; set; }
}

public class AiAnalyticsDto
{
    public int TotalChats { get; set; }
    public int TotalTokensUsed { get; set; }
    public int TotalCraftsmenIngested { get; set; }
    public int TotalSolutionsIngested { get; set; }
    public double AverageTokensPerChat { get; set; }
}

public class ReviewAnalyticsDto
{
    public int TotalReviews { get; set; }
    public double AverageStars { get; set; }
    public Dictionary<int, int> StarDistribution { get; set; } = new();
    public int DeletedReviews { get; set; }
}

public class AiLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ToolUsed { get; set; }
    public int? TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
