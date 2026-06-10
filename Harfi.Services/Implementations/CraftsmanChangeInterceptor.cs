using Harfi.DTOs.RAG; // أو الـ namespace اللي فيه SeedStatus
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

        private readonly List<(int CraftsmanId, EntityState State)> _changes = new();

        public CraftsmanChangeInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // أثناء الـ Seeding متعملش أي حاجة
            if (!SeedStatus.IsCompleted)
            {
                return base.SavingChangesAsync(
                    eventData,
                    result,
                    cancellationToken);
            }

            var context = eventData.Context;

            if (context != null)
            {
                _changes.Clear();

                _changes.AddRange(
                    context.ChangeTracker
                        .Entries<Craftsman>()
                        .Where(e =>
                            e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                        .Select(e => (e.Entity.Id, e.State))
                );
            }

            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            // أثناء الـ Seeding متعملش أي حاجة
            if (!SeedStatus.IsCompleted)
            {
                return await base.SavedChangesAsync(
                    eventData,
                    result,
                    cancellationToken);
            }

            if (!_changes.Any())
            {
                return await base.SavedChangesAsync(
                    eventData,
                    result,
                    cancellationToken);
            }

            using var scope = _serviceProvider.CreateScope();

            var ragService =
                scope.ServiceProvider.GetRequiredService<RAGService>();

            var logger =
                scope.ServiceProvider.GetRequiredService<
                    ILogger<CraftsmanChangeInterceptor>>();

            foreach (var (craftsmanId, state) in _changes)
            {
                try
                {
                    switch (state)
                    {
                        case EntityState.Added:
                        case EntityState.Modified:

                            await ragService
                                .UpsertCraftsmanToVectorDbAsync(craftsmanId);

                            logger.LogInformation(
                                "Craftsman {CraftsmanId} synced to Qdrant",
                                craftsmanId);

                            break;

                        case EntityState.Deleted:

                            // لو عندك ميثود حذف فعلها هنا
                            // await ragService.DeleteCraftsmanFromVectorDbAsync(craftsmanId);

                            logger.LogInformation(
                                "Craftsman {CraftsmanId} deleted from Qdrant",
                                craftsmanId);

                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed syncing Craftsman {CraftsmanId}",
                        craftsmanId);
                }
            }

            _changes.Clear();

            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        public override async Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _changes.Clear();

            await base.SaveChangesFailedAsync(
                eventData,
                cancellationToken);
        }
    }
}