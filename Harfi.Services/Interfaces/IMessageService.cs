using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> SaveMessageAsync(int conversationId, int senderId, string content, string messageType);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId, int page, int pageSize);
        Task MarkConversationAsReadAsync(int conversationId, int userId);
        Task<bool> DeleteMessageAsync(int conversationId, int messageId, int userId);
    }
}
