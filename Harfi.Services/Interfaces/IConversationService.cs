using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces
{
    public interface IConversationService
    {
        Task<ConversationDto> GetOrCreateAsync(int jobId, int customerId, int craftsmanId);
        Task<bool> IsParticipantAsync(int conversationId, int userId);
        Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId);
        Task<ConversationDto?> GetByIdAsync(int conversationId, int userId);
    }
}
