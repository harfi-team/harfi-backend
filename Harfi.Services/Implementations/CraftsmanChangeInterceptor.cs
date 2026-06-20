
using Harfi.DTOs.RAG;
using Harfi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations
{
    public class CraftsmanChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        // ThreadLocal علشان كل request يبقى عنده list منفصلة
        private readonly AsyncLocal<List<(int CraftsmanId, EntityState State)>> _changes = new();

        private List<(int CraftsmanId, EntityState State)> Changes =>
            _changes.Value ??= new List<(int, EntityState)>();

        public CraftsmanChangeInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!SeedStatus.IsCompleted)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var context = eventData.Context;
            if (context != null)
            {
                Changes.Clear();
                Changes.AddRange(
                    context.ChangeTracker
                        .Entries<Craftsman>()
                        .Where(e =>
                            e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                        .Select(e => (e.Entity.Id, e.State))
                );
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (!SeedStatus.IsCompleted)
                return await base.SavedChangesAsync(eventData, result, cancellationToken);

            if (!Changes.Any())
                return await base.SavedChangesAsync(eventData, result, cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var ragService = scope.ServiceProvider.GetRequiredService<RAGService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CraftsmanChangeInterceptor>>();

            foreach (var (craftsmanId, state) in Changes.ToList())
            {
                try
                {
                    switch (state)
                    {
                        case EntityState.Added:
                        case EntityState.Modified:
                            await ragService.UpsertCraftsmanToVectorDbAsync(craftsmanId);
                            logger.LogInformation("[CDC] ✓ Craftsman {Id} upserted in Qdrant", craftsmanId);
                            break;

                        case EntityState.Deleted:
                            await ragService.DeleteCraftsmanFromVectorDbAsync(craftsmanId);
                            logger.LogInformation("[CDC] ✓ Craftsman {Id} deleted from Qdrant", craftsmanId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[CDC] ✗ Failed syncing Craftsman {Id}", craftsmanId);
                }
            }

            Changes.Clear();
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override async Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Changes.Clear();
            await base.SaveChangesFailedAsync(eventData, cancellationToken);
        }
    }
}