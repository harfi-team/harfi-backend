using Harfi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
                Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> DeleteAsync(int notificationId, int userId);
        Task<int> DeleteAllAsync(int userId);
        Task CreateAsync(Notification notification);

    }
}
