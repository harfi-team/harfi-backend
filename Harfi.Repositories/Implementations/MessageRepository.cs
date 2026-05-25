using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetByConversationAsync(
            int conversationId, int page, int pageSize)
            => await _dbSet
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task MarkConversationAsReadAsync(int conversationId, int userId)
        {
            var unread = await _dbSet
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    m.IsRead == false)
                .ToListAsync();

            unread.ForEach(m => m.IsRead = true);
            await SaveChangesAsync();
        }

        public Task<int> GetUnreadCountAsync(int conversationId, int userId)
            => CountAsync(m =>
                m.ConversationId == conversationId &&
                m.SenderId != userId &&
                m.IsRead == false);
    }
}
