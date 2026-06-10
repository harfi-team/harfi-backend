using Harfi.API.Hubs;
using Harfi.DTOs.Chat;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Harfi.API.Hubs;

public class SignalRNotificationPusher : IRealtimeNotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushAsync(int userId, NotificationDto notification)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }
}
