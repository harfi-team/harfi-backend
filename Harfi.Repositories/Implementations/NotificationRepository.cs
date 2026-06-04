using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        => await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await FirstOrDefaultAsync(n =>
            n.Id == notificationId &&
            n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _dbSet
            .Where(n => n.UserId == userId && n.IsRead == false)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await SaveChangesAsync();
    }

    public Task<int> GetUnreadCountAsync(int userId)
        => CountAsync(n => n.UserId == userId && n.IsRead == false);
    public async Task CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}