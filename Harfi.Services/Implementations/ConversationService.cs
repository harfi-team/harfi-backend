using Harfi.DTOs.Chat;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _convRepo;
        private readonly IMessageRepository _msgRepo;
        private readonly IJobRepository _jobRepository;

        public ConversationService(
            IConversationRepository convRepo,
            IMessageRepository msgRepo,
            IJobRepository jobRepository)
        {
            _convRepo = convRepo;
            _msgRepo = msgRepo;
            _jobRepository = jobRepository;
        }


        public async Task<ConversationDto> GetOrCreateAsync(
            int jobId, int customerId, int craftsmanId)
        {
            var existing = await _convRepo
                .GetByParticipantsAsync(jobId, customerId, craftsmanId);

            if (existing != null)
                return await MapToDtoAsync(existing, customerId);

            var created = await _convRepo.AddAsync(new Conversation
            {
                JobId = jobId,
                CustomerId = customerId,
                CraftsmanId = craftsmanId
            });
            await _convRepo.SaveChangesAsync();

            var full = await _convRepo.GetByIdWithDetailsAsync(created.Id);
            return await MapToDtoAsync(full!, customerId);
        }

        public Task<bool> IsParticipantAsync(int conversationId, int userId)
            => _convRepo.IsParticipantAsync(conversationId, userId);

        public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId)
        {
            var conversations = await _convRepo.GetUserConversationsAsync(userId);
            var convIds = conversations.Select(c => c.Id).ToList();

            // Single query for all unread counts — avoids N+1
            var unreadCounts = await _msgRepo.GetBatchUnreadCountsAsync(convIds, userId);

            return conversations.Select(c =>
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
                    LastMessageAt = lastMsg?.SentAt,
                    UnreadCount = unreadCounts.GetValueOrDefault(c.Id, 0)
                };
            });
        }

        public async Task<ConversationDto?> GetByIdAsync(int conversationId, int userId)
        {
            var c = await _convRepo.GetByIdWithDetailsAsync(conversationId);
            if (c == null) return null;
            if (c.CustomerId != userId && c.Craftsman?.UserId != userId) return null;
            return await MapToDtoAsync(c, userId);
        }

        public async Task ValidateJobForConversationAsync(
            int jobId, int? requestedCraftsmanId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId)
                ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

            if (job.Status == JobStatusConstants.Rejected ||
                job.Status == JobStatusConstants.Cancelled)
                throw new InvalidOperationException(
                    $"لا يمكن بدء محادثة على وظيفة {job.Status}.");

            if (job.CraftsmanId.HasValue &&
                requestedCraftsmanId.HasValue &&
                job.CraftsmanId != requestedCraftsmanId)
                throw new InvalidOperationException(
                    "الحرفي المحدد لا ينتمي لهذه الوظيفة.");
        }

        // ── Mapper ────────────────────────────────────────────────
        private async Task<ConversationDto> MapToDtoAsync(Conversation c, int userId)
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
            var unreadCount = await _msgRepo.GetUnreadCountAsync(c.Id, userId);

            return new ConversationDto
            {
                Id = c.Id,
                JobId = c.JobId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                OtherUserAvatar = otherUserAvatar,
                LastMessage = lastMsg?.Content,
                LastMessageAt = lastMsg?.SentAt,
                UnreadCount = unreadCount
            };
        }
    }
}
