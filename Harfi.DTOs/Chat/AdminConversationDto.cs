namespace Harfi.DTOs.Chat;

public class AdminConversationDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CraftsmanName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminConversationDetailDto : AdminConversationDto
{
    public List<MessageDto> Messages { get; set; } = new();
}

public class ConversationFilterDto
{
    public string? CustomerName { get; set; }
    public string? CraftsmanName { get; set; }
    public string? ServiceType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
