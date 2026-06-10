namespace Harfi.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int adminId, string action, string targetType, int targetId, string? notes = null, string? ipAddress = null);
}
