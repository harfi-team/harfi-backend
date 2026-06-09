using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifRepo;

        public NotificationService(INotificationRepository notifRepo)
        {
            _notifRepo = notifRepo;
        }

        public async Task<NotificationDto> CreateMessageNotificationAsync(
            int receiverId, string senderName, string messagePreview, string messageType, int conversationId)
        {
            var preview = BuildMessagePreview(messagePreview, messageType);

            var notif = new Notification
            {
                UserId = receiverId,
                Title = $"رسالة جديدة من {senderName}",
                Body = preview,
                Type = "new_message",
                ConversationId = conversationId
            };
            await _notifRepo.AddAsync(notif);
            await _notifRepo.SaveChangesAsync();
            return MapToDto(notif);
        }

        private static string BuildMessagePreview(string content, string messageType)
        {
            var normalizedType = messageType?.Trim().ToLowerInvariant();

            return normalizedType switch
            {
                "image" => "📷 صورة",
                "voice" => "🎤 رسالة صوتية",
                "location" => "📍 موقع",
                _ => BuildTextPreview(content)
            };
        }

        private static string BuildTextPreview(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "رسالة جديدة";
            }

            return content.Length > 80
                ? content[..80] + "..."
                : content;
        }

        public async Task<NotificationDto> CreateJobNotificationAsync(
            int receiverId, string title, string body, string type, int relatedJobId)
        {
            var notif = new Notification
            {
                UserId = receiverId,
                Title = title,
                Body = body,
                Type = type,
                RelatedJobId = relatedJobId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notifRepo.AddAsync(notif);
            await _notifRepo.SaveChangesAsync();
            return MapToDto(notif);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notifRepo.GetUserNotificationsAsync(userId);
            return notifications.Select(MapToDto);
        }

        public Task<bool> MarkAsReadAsync(int notificationId, int userId)
            => _notifRepo.MarkAsReadAsync(notificationId, userId);

        public Task MarkAllAsReadAsync(int userId)
            => _notifRepo.MarkAllAsReadAsync(userId);

        public Task<int> GetUnreadCountAsync(int userId)
            => _notifRepo.GetUnreadCountAsync(userId);

        public Task<bool> DeleteAsync(int notificationId, int userId)
            => _notifRepo.DeleteAsync(notificationId, userId);

        public Task<int> DeleteAllAsync(int userId)
            => _notifRepo.DeleteAllAsync(userId);

        // ── Mapper ────────────────────────────────────────────────
        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            RelatedJobId = n.RelatedJobId,
            ConversationId = n.ConversationId,
            IsRead = n.IsRead,
            CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc)
        };
    }
}
