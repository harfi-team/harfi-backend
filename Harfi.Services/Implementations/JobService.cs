using Harfi.DTOs.Job;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly INotificationRepository _notificationRepository;

    public JobService(IJobRepository jobRepository, INotificationRepository notificationRepository)
    {
        _jobRepository = jobRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<JobResponseDto> CreateJobAsync(int customerId, CreateJobDto dto)
    {
        var job = new Job
        {
            CustomerId = customerId,
            CraftsmanId = dto.CraftsmanId,
            Status = "open",
            ServiceType = dto.ServiceType,
            Description = dto.Description,
            Address = dto.Address,
            PreferredDate = dto.PreferredDate,
            ProblemImageUrl = dto.ProblemImageUrl,
            ProblemDescription = dto.ProblemDescription,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _jobRepository.CreateAsync(job);
        return MapToDto(created);
    }

    public async Task<JobResponseDto> AcceptJobAsync(int jobId, int craftsmanId)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, "open");
        job.Status = "in-progress";

        var updated = await _jobRepository.UpdateAsync(job);

        // notify customer
        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = job.CustomerId,
            Title = "تم قبول طلبك",
            Body = "قام الحرفي بقبول طلب الخدمة الخاص بك",
            IsRead = false,
            Type = "job_accepted",
            RelatedJobId = job.Id,
            CreatedAt = DateTime.UtcNow
        });

        return MapToDto(updated);
    }

    public async Task<JobResponseDto> RejectJobAsync(int jobId, int craftsmanId)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, "open");
        job.Status = "rejected";

        var updated = await _jobRepository.UpdateAsync(job);

        // notify customer
        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = job.CustomerId,
            Title = "تم رفض طلبك",
            Body = "قام الحرفي برفض طلب الخدمة الخاص بك",
            IsRead = false,
            Type = "job_rejected",
            RelatedJobId = job.Id,
            CreatedAt = DateTime.UtcNow
        });

        return MapToDto(updated);
    }

    public async Task<JobResponseDto> CompleteJobAsync(int jobId, int craftsmanId, UpdateJobStatusDto dto)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, "in-progress");
        job.Status = "done";
        job.SolutionDescription = dto.SolutionDescription;
        job.CompletedAt = DateTime.UtcNow;

        var updated = await _jobRepository.UpdateAsync(job);

        // notify customer
        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = job.CustomerId,
            Title = "تم إنجاز طلبك",
            Body = "قام الحرفي بإنهاء العمل. يمكنك الآن تقييم الخدمة",
            IsRead = false,
            Type = "job_completed",
            RelatedJobId = job.Id,
            CreatedAt = DateTime.UtcNow
        });

        return MapToDto(updated);
    }

    public async Task<IEnumerable<JobResponseDto>> GetCustomerJobsAsync(int customerId)
    {
        var jobs = await _jobRepository.GetByCustomerIdAsync(customerId);
        return jobs.Select(MapToDto);
    }

    public async Task<IEnumerable<JobResponseDto>> GetCraftsmanJobsAsync(int craftsmanId)
    {
        var jobs = await _jobRepository.GetByCraftsmanIdAsync(craftsmanId);
        return jobs.Select(MapToDto);
    }

    // ─── Private Helpers ────────────────────────────────────────────────────

    private async Task<Job> GetAndValidateJob(int jobId, int craftsmanUserId, string requiredStatus)
    {
        var job = await _jobRepository.GetByIdAsync(jobId)
            ?? throw new KeyNotFoundException("الطلب غير موجود");

        // Get craftsman record by UserId (from token) to find Craftsmen.Id
        var craftsman = await _jobRepository.GetCraftsmanByUserIdAsync(craftsmanUserId)
            ?? throw new UnauthorizedAccessException("ليس لديك صلاحية تعديل هذا الطلب");

        if (job.CraftsmanId != craftsman.Id)
            throw new UnauthorizedAccessException("ليس لديك صلاحية تعديل هذا الطلب");

        if (job.Status != requiredStatus)
            throw new InvalidOperationException($"لا يمكن تنفيذ هذا الإجراء. الحالة الحالية: {job.Status}");

        return job;
    }

    private static JobResponseDto MapToDto(Job job) => new()
    {
        Id = job.Id,
        CustomerId = job.CustomerId,
        CraftsmanId = job.CraftsmanId,
        Status = job.Status,
        ServiceType = job.ServiceType,
        Description = job.Description,
        Address = job.Address,
        PreferredDate = job.PreferredDate,
        ProblemImageUrl = job.ProblemImageUrl,
        ProblemDescription = job.ProblemDescription,
        SolutionDescription = job.SolutionDescription,
        CreatedAt = job.CreatedAt,
        CompletedAt = job.CompletedAt,
        UpdatedAt = job.UpdatedAt
    };
}