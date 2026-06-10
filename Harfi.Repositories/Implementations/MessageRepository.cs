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
            await _dbSet
                .Where(m => m.ConversationId == conversationId
                         && m.SenderId != userId
                         && !m.IsRead)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(m => m.IsRead, true));
        }

        public Task<int> GetUnreadCountAsync(int conversationId, int userId)
            => CountAsync(m =>
                m.ConversationId == conversationId &&
                m.SenderId != userId &&
                m.IsRead == false);

        public async Task<Dictionary<int, int>> GetBatchUnreadCountsAsync(
            List<int> conversationIds, int userId)
            => await _dbSet
                .Where(m => conversationIds.Contains(m.ConversationId)
                         && m.SenderId != userId
                         && !m.IsRead)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConvId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConvId, x => x.Count);
    }
}
