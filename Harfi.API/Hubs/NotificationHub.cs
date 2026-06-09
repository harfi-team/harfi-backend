using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Harfi.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
                }
            }
            catch
            {
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            
            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
                }
            }
            catch
            {
            }

            await base.OnDisconnectedAsync(exception);
        }

        private int? GetUserId()
        {
            try
            {
                var claimValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claimValue) && int.TryParse(claimValue, out int userId))
                {
                    return userId;
                }
            }
            catch
            {
            }
            return null;
        }
    }
}