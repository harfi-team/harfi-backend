using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<Conversation?> GetByParticipantsAsync(int jobId, int customerId, int craftsmanId);
        Task<Conversation?> GetByIdWithDetailsAsync(int id);
        Task<Conversation?> GetByIdWithMessagesAsync(int id);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<bool> IsParticipantAsync(int conversationId, int userId);
        IQueryable<Conversation> GetAllConversationsQuery();
        IQueryable<Conversation> GetAllConversationsQueryIgnoreFilters();
    }
}
