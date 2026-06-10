using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces;

public interface IRealtimeNotificationPusher
{
    Task PushAsync(int userId, NotificationDto notification);
}
