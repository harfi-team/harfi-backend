using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateMessageNotificationAsync(int receiverId, string senderName, string messagePreview);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
