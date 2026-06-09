using Harfi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Harfi.API.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();
                var deleted = await db.RefreshTokens
                    .Where(rt => rt.ExpiresAt < DateTime.UtcNow 
                              || rt.IsRevoked)
                    .ExecuteDeleteAsync(stoppingToken);
                _logger.LogInformation(
                    "RefreshToken cleanup: deleted {Count} tokens.", deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken cleanup failed.");
            }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
