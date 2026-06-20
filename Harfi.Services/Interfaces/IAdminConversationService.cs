using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces;

public interface IAdminConversationService
{
    Task<IEnumerable<AdminConversationDto>> GetAllConversationsAsync(ConversationFilterDto filter);
    Task<AdminConversationDetailDto?> GetConversationWithMessagesAsync(int conversationId);
}
