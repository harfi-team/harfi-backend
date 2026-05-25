using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(int id);
    Task<IEnumerable<Job>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Job>> GetByCraftsmanIdAsync(int craftsmanId);
    Task<Job> CreateAsync(Job job);
    Task<Job> UpdateAsync(Job job);
    Task<Craftsman?> GetCraftsmanByUserIdAsync(int userId);
}