using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Implementations
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(AppDbContext context) : base(context) { }

        public async Task<Conversation?> GetByParticipantsAsync(
            int jobId, int customerId, int craftsmanId)
            => await _dbSet
                .FirstOrDefaultAsync(c =>
                    c.JobId == jobId &&
                    c.CustomerId == customerId &&
                    c.CraftsmanId == craftsmanId);

        public async Task<Conversation?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(c => c.Customer)
                .Include(c => c.Craftsman)
                    .ThenInclude(cr => cr.User)
                .Include(c => c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Take(1))
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
            => await _dbSet
                .Include(c => c.Customer)
                .Include(c => c.Craftsman)
                   .ThenInclude(cr => cr.User)
                .Include(c => c.Messages)
            .Where(c => c.CustomerId == userId || c.Craftsman.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

        public async Task<bool> IsParticipantAsync(int conversationId, int userId)
         => await _dbSet
            .Include(c => c.Craftsman)
            .AnyAsync(c =>
                c.Id == conversationId &&
                 (c.CustomerId == userId || c.Craftsman.UserId == userId));
    }
}
