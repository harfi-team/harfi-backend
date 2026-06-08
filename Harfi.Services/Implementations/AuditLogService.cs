using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IGenericRepository<AdminAuditLog> _auditLogRepo;

    public AuditLogService(IGenericRepository<AdminAuditLog> auditLogRepo)
    {
        _auditLogRepo = auditLogRepo;
    }

    public async Task LogAsync(int adminId, string action, string targetType, int targetId, string? notes = null, string? ipAddress = null)
    {
        await _auditLogRepo.AddAsync(new AdminAuditLog
        {
            AdminId = adminId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Notes = notes,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _auditLogRepo.SaveChangesAsync();
    }
}
