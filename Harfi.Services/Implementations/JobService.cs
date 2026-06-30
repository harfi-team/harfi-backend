using Harfi.DTOs.Job;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Services.Implementations;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly INotificationService _notificationService;
    private readonly ICraftsmanRepository _craftsmanRepo;
    private readonly IRealtimeNotificationPusher _notifPusher;
    private readonly AppDbContext _db;

    public JobService(
    IJobRepository jobRepository,
    INotificationService notificationService,
    ICraftsmanRepository craftsmanRepo,
    IRealtimeNotificationPusher notifPusher,
    AppDbContext db)
    {
        _jobRepository = jobRepository;
        _notificationService = notificationService;
        _craftsmanRepo = craftsmanRepo;
        _notifPusher = notifPusher;
        _db = db;
    }

    public async Task<JobResponseDto> CreateJobAsync(int customerId, CreateJobDto dto)
    {
        var job = new Job
        {
            CustomerId = customerId,
            CraftsmanId = dto.CraftsmanId,
            Status = JobStatusConstants.Open,
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

        var craftsman = await _craftsmanRepo.GetByIdAsync(dto.CraftsmanId ?? 0);
        if (craftsman != null)
        {
            var notifDto = await _notificationService.CreateJobNotificationAsync(
                craftsman.UserId, "طلب خدمة جديد",
                $"قام أحد العملاء بطلب خدمة {dto.ServiceType} جديدة",
                "new_order", created.Id);
            await _notifPusher.PushAsync(craftsman.UserId, notifDto);
        }

        return MapToDto(created);
    }

    public async Task<JobResponseDto> AcceptJobAsync(int jobId, int craftsmanId)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, JobStatusConstants.Open);
        job.Status = JobStatusConstants.InProgress;

        var updated = await _jobRepository.UpdateAsync(job);

        // auto-create conversation if none exists
        var existingConv = await _db.Conversations
            .FirstOrDefaultAsync(c => c.JobId == job.Id);
        if (existingConv == null)
        {
            var newConv = new Conversation
            {
                JobId = job.Id,
                CustomerId = job.CustomerId,
                CraftsmanId = job.CraftsmanId!.Value,
                CreatedAt = DateTime.UtcNow
            };
            _db.Conversations.Add(newConv);
            await _db.SaveChangesAsync();
            job.Conversation = newConv;
        }

        // notify customer
        var notifAccepted = await _notificationService.CreateJobNotificationAsync(
        job.CustomerId, "تم قبول طلبك",
        "قام الحرفي بقبول طلب الخدمة الخاص بك",
        "job_accepted", job.Id);
        await _notifPusher.PushAsync(job.CustomerId, notifAccepted);

        return MapToDto(job);

    }

    public async Task<JobResponseDto> RejectJobAsync(int jobId, int craftsmanId)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, JobStatusConstants.Open);
        job.Status = JobStatusConstants.Rejected;

        var updated = await _jobRepository.UpdateAsync(job);

        // notify customer
        var notifRejected = await _notificationService.CreateJobNotificationAsync(
        job.CustomerId, "تم رفض طلبك",
        "قام الحرفي برفض طلب الخدمة الخاص بك",
        "job_rejected", job.Id);
        await _notifPusher.PushAsync(job.CustomerId, notifRejected);

        return MapToDto(updated);
    }

    public async Task<JobResponseDto> CompleteJobAsync(int jobId, int craftsmanId, UpdateJobStatusDto dto)
    {
        var job = await GetAndValidateJob(jobId, craftsmanId, JobStatusConstants.InProgress);
        job.Status = JobStatusConstants.Done;
        job.SolutionDescription = dto.SolutionDescription;
        job.CompletedAt = DateTime.UtcNow;

        var updated = await _jobRepository.UpdateAsync(job);

        // notify customer
        var notifCompleted = await _notificationService.CreateJobNotificationAsync(
        job.CustomerId, "تم إنجاز طلبك",
        "قام الحرفي بإنهاء العمل. يمكنك الآن تقييم الخدمة",
        "job_completed", job.Id);
        await _notifPusher.PushAsync(job.CustomerId, notifCompleted);

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

    public async Task<bool> CraftsmanBelongsToUserAsync(int craftsmanId, int userId)
        => await _jobRepository.CraftsmanBelongsToUserAsync(craftsmanId, userId);

    public async Task<JobResponseDto?> GetJobByIdAsync(int jobId, int userId, string role)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null) return null;

        if (role == "admin")
            return MapToDto(job);

        if (role == "customer")
        {
            if (job.CustomerId != userId)
                return null;
            return MapToDto(job);
        }

        if (role == "craftsman")
        {
            var craftsman = await _jobRepository.GetCraftsmanByUserIdAsync(userId);
            if (craftsman == null || job.CraftsmanId != craftsman.Id)
                return null;
            return MapToDto(job);
        }

        return null;
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

    private static JobResponseDto MapToDto(Job job)
    {
        var activeDispute = job.Disputes?
            .FirstOrDefault(d => DisputeStatusConstants.Active.Contains(d.Status));
        var resolvedDispute = job.Disputes?
            .FirstOrDefault(d => d.Status == DisputeStatusConstants.Resolved);

        return new JobResponseDto
        {
            Id = job.Id,
            CustomerId = job.CustomerId,
            CustomerName = job.Customer?.Name,
            CraftsmanId = job.CraftsmanId,
            CraftsmanName = job.Craftsman?.User?.Name,
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
            UpdatedAt = job.UpdatedAt,
            ConversationId = job.Conversation?.Id,
            HasOpenDispute = activeDispute != null,
            DisputeStatus = activeDispute?.Status ?? resolvedDispute?.Status
        };
    }
}