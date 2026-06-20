using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Services.Implementations
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _convRepo;
        private readonly IMessageRepository _msgRepo;
        private readonly AppDbContext _db;

        public ConversationService(
            IConversationRepository convRepo,
            IMessageRepository msgRepo,
            AppDbContext db)
        {
            _convRepo = convRepo;
            _msgRepo = msgRepo;
            _db = db;
        }

       public async Task<ConversationDto> GetOrCreateAsync(int jobId, int customerId, int craftsmanId)
{
    var existing = await _db.Conversations
        .Include(c => c.Messages)
        .Include(c => c.Customer)
        .Include(c => c.Craftsman).ThenInclude(cr => cr.User)
        .FirstOrDefaultAsync(c =>
            c.JobId == jobId &&
            c.CustomerId == customerId &&
            c.CraftsmanId == craftsmanId); // ✅ مباشرة بدون lookup

    if (existing != null)
        return await MapToDtoAsync(existing, customerId);

    var conversation = new Conversation
    {
        JobId = jobId,
        CustomerId = customerId,
        CraftsmanId = craftsmanId, // ✅ Craftsman.Id مباشرة
        CreatedAt = DateTime.UtcNow
    };

    _db.Conversations.Add(conversation);
    await _db.SaveChangesAsync();

    var created = await _db.Conversations
        .Include(c => c.Messages)
        .Include(c => c.Customer)
        .Include(c => c.Craftsman).ThenInclude(cr => cr.User)
        .FirstAsync(c => c.Id == conversation.Id);

    return await MapToDtoAsync(created, customerId);
}

        public Task<bool> IsParticipantAsync(int conversationId, int userId)
            => _convRepo.IsParticipantAsync(conversationId, userId);

        public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId)
        {
            var conversations = (await _convRepo.GetUserConversationsAsync(userId)).ToList();
            if (conversations.Count == 0) return [];

            var conversationIds = conversations.Select(c => c.Id).ToList();
            var unreadCounts = await _db.Messages
                .Where(m =>
                    conversationIds.Contains(m.ConversationId) &&
                    m.SenderId != userId &&
                    !m.IsRead)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

            var otherUserIds = conversations
                .Select(c => c.CustomerId == userId ? c.Craftsman.UserId : c.CustomerId)
                .Distinct()
                .ToList();

            var onlineUserIds = await _db.UserConnections
                .Where(uc => otherUserIds.Contains(uc.UserId) && uc.IsConnected)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToListAsync();
            var onlineSet = onlineUserIds.ToHashSet();

            return conversations.Select(c =>
            {
                var otherUserId = c.CustomerId == userId ? c.Craftsman.UserId : c.CustomerId;
                unreadCounts.TryGetValue(c.Id, out var unreadCount);
                return BuildConversationDto(c, userId, unreadCount, onlineSet.Contains(otherUserId));
            });
        }

        public async Task<ConversationDto?> GetByIdAsync(int conversationId, int userId)
        {
            var c = await _convRepo.GetByIdIfVisibleAsync(conversationId, userId);
            if (c == null) return null;
            return await MapToDtoAsync(c, userId);
        }

        public async Task<bool> HideConversationAsync(int conversationId, int userId)
        {
            var c = await _convRepo.GetByIdWithDetailsAsync(conversationId);
            if (c == null) return false;

            if (c.CustomerId == userId)
                c.CustomerHiddenAt = DateTime.UtcNow;
            else if (c.Craftsman.UserId == userId)
                c.CraftsmanHiddenAt = DateTime.UtcNow;
            else
                return false;

            c.UpdatedAt = DateTime.UtcNow;
            _convRepo.Update(c);
            await _convRepo.SaveChangesAsync();
            return true;
        }

        // ── Mappers ───────────────────────────────────────────────
        private async Task<ConversationDto> MapToDtoAsync(Conversation c, int userId)
        {
            var isCustomer = c.CustomerId == userId;
            var otherUserId = isCustomer ? c.Craftsman.UserId : c.CustomerId;
            var unreadCount = await _msgRepo.GetUnreadCountAsync(c.Id, userId);
            var isOnline = await _db.UserConnections
                .AnyAsync(uc => uc.UserId == otherUserId && uc.IsConnected);

            return BuildConversationDto(c, userId, unreadCount, isOnline);
        }

        private static ConversationDto BuildConversationDto(
            Conversation c,
            int userId,
            int unreadCount,
            bool isOnline)
        {
            var isCustomer = c.CustomerId == userId;
            var otherUserId = isCustomer ? c.Craftsman.UserId : c.CustomerId;
            var otherUserName = isCustomer
                ? (c.Craftsman?.User?.Name ?? string.Empty)
                : (c.Customer?.Name ?? string.Empty);
            var otherUserAvatar = isCustomer
                ? c.Craftsman?.User?.ProfileImageUrl
                : c.Customer?.ProfileImageUrl;

            var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

            return new ConversationDto
            {
                Id = c.Id,
                JobId = c.JobId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                OtherUserAvatar = otherUserAvatar,
                LastMessage = lastMsg?.Content,
                LastMessageType = lastMsg?.MessageType,
                LastMessageAt = lastMsg?.SentAt is DateTime dt
                    ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                    : (DateTime?)null,
                UnreadCount = unreadCount,
                IsOnline = isOnline
            };
        }
    }
}