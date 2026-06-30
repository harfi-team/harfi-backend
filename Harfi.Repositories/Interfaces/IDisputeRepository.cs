using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces;

public interface IDisputeRepository
{
    Task<Dispute?> GetByIdAsync(int id);
    Task<Dispute?> GetActiveDisputeForJobAsync(int jobId);
    Task<bool> HasActiveDisputeAsync(int jobId);
    Task<IEnumerable<Dispute>> GetByUserIdAsync(int userId);
    Task<Dispute> CreateAsync(Dispute dispute);
    Task<Dispute> UpdateAsync(Dispute dispute);
    Task<int> CountActiveAsync();
    IQueryable<Dispute> GetQueryable();
}
