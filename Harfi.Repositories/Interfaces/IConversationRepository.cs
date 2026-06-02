using Harfi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<Conversation?> GetByParticipantsAsync(int jobId, int customerId, int craftsmanId);
        Task<Conversation?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<bool> IsParticipantAsync(int conversationId, int userId);
    }
}
