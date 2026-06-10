namespace Harfi.DTOs.Admin;

public class ReviewAdminDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CraftsmanId { get; set; }
    public string CraftsmanName { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewAdminDetailDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CraftsmanId { get; set; }
    public string CraftsmanName { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletionReason { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeleteReviewRequest
{
    public string Reason { get; set; } = string.Empty;
}
