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

        public async Task CreateMessageNotificationAsync(
            int receiverId, string senderName, string messagePreview)
        {
            var preview = messagePreview.Length > 80
                ? messagePreview[..80] + "..."
                : messagePreview;

            await _notifRepo.AddAsync(new Notification
            {
                UserId = receiverId,
                Title = $"رسالة جديدة من {senderName}",
                Body = preview,
                Type = "new_message"
            });
            await _notifRepo.SaveChangesAsync();
        }

        public async Task CreateJobNotificationAsync(
            int receiverId, string title, string body, string type, int relatedJobId)
        {
            await _notifRepo.AddAsync(new Notification
            {
                UserId = receiverId,
                Title = title,
                Body = body,
                Type = type,
                RelatedJobId = relatedJobId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _notifRepo.SaveChangesAsync();
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

        // ── Mapper ────────────────────────────────────────────────
        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            RelatedJobId = n.RelatedJobId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}