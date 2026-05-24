namespace Harfi.Models.Entities;

public class EmailVerification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}