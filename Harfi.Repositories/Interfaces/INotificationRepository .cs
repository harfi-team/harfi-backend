using Harfi.Models.Entities;

public interface INotificationRepository
{
    Task CreateAsync(Notification notification);
}