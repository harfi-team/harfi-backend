using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces
{
    public interface INotificationService
    {
                Task<NotificationDto> CreateMessageNotificationAsync(int receiverId, string senderName, string messagePreview, string messageType, int conversationId);

        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
                Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> DeleteAsync(int notificationId, int userId);
        Task<int> DeleteAllAsync(int userId);

        Task<NotificationDto> CreateJobNotificationAsync(int receiverId, string title, string body, string type, int relatedJobId);
    }
}
