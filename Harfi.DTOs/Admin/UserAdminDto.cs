using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Admin;

public class UserAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserAdminDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? DeletionReason { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CraftsmanProfileId { get; set; }
    public int JobsCount { get; set; }
    public int ReviewsCount { get; set; }
}

public class UserActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DeactivateUserRequest
{
    [Required(ErrorMessage = "سبب التعطيل مطلوب")]
    [MinLength(10, ErrorMessage = "يجب أن يكون سبب التعطيل 10 أحرف على الأقل")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class DeleteUserRequest
{
    [Required(ErrorMessage = "سبب الحذف مطلوب")]
    [MinLength(10, ErrorMessage = "يجب أن يكون سبب الحذف 10 أحرف على الأقل")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
