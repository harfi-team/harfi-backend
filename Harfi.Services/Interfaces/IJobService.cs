using Harfi.DTOs.Job;

namespace Harfi.Services.Interfaces;

public interface IJobService
{
    Task<JobResponseDto> CreateJobAsync(int customerId, CreateJobDto dto);
    Task<JobResponseDto> AcceptJobAsync(int jobId, int craftsmanId);
    Task<JobResponseDto> RejectJobAsync(int jobId, int craftsmanId);
    Task<JobResponseDto> CompleteJobAsync(int jobId, int craftsmanId, UpdateJobStatusDto dto);
    Task<IEnumerable<JobResponseDto>> GetCustomerJobsAsync(int customerId);
    Task<IEnumerable<JobResponseDto>> GetCraftsmanJobsAsync(int craftsmanId);
    Task<bool> CraftsmanBelongsToUserAsync(int craftsmanId, int userId);
}