using Harfi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetByConversationAsync(int conversationId, int page, int pageSize);
        Task MarkConversationAsReadAsync(int conversationId, int userId);
        Task<int> GetUnreadCountAsync(int conversationId, int userId);
    }
}
