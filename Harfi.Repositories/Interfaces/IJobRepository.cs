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
    Task<bool> CraftsmanBelongsToUserAsync(int craftsmanId, int userId);
    Task<IEnumerable<Job>> GetCompletedJobsWithSolutionsAsync();
    Task<Job?> GetJobWithReviewAndCraftsmanAsync(int jobId);
}