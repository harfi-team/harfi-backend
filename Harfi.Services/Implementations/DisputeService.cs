using Harfi.DTOs.Dispute;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations;

public class DisputeService : IDisputeService
{
    private readonly IDisputeRepository _disputeRepo;
    private readonly IJobRepository _jobRepo;
    private readonly ICraftsmanRepository _craftsmanRepo;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeNotificationPusher _notifPusher;

    public DisputeService(
        IDisputeRepository disputeRepo,
        IJobRepository jobRepo,
        ICraftsmanRepository craftsmanRepo,
        INotificationService notificationService,
        IRealtimeNotificationPusher notifPusher)
    {
        _disputeRepo = disputeRepo;
        _jobRepo = jobRepo;
        _craftsmanRepo = craftsmanRepo;
        _notificationService = notificationService;
        _notifPusher = notifPusher;
    }

    public async Task<DisputeDetailDto> OpenDisputeAsync(int jobId, int userId, string role, CreateDisputeRequest dto)
    {
        var job = await _jobRepo.GetByIdAsync(jobId)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        // Validate the user is a party to this job
        if (role == "customer" && job.CustomerId != userId)
            throw new UnauthorizedAccessException("لا يمكنك فتح نزاع على وظيفة ليست مملوكة لك.");
        if (role == "craftsman")
        {
            var craftsman = await _craftsmanRepo.GetByUserIdAsync(userId);
            if (craftsman == null || job.CraftsmanId != craftsman.Id)
                throw new UnauthorizedAccessException("لا يمكنك فتح نزاع على وظيفة غير مخصصة لك.");
        }

        // Validate job status allows dispute
        var allowedStatuses = new[] { JobStatusConstants.InProgress, JobStatusConstants.Done };
        if (!allowedStatuses.Contains(job.Status))
            throw new InvalidOperationException(
                "لا يمكن فتح نزاع إلا على وظائف قيد التنفيذ أو مكتملة.");

        // Check no active dispute
        if (await _disputeRepo.HasActiveDisputeAsync(jobId))
            throw new InvalidOperationException("يوجد بالفعل نزاع نشط على هذه الوظيفة.");

        var dispute = new Dispute
        {
            JobId = jobId,
            RaisedByUserId = userId,
            RaisedByRole = role,
            Reason = dto.Reason,
            Description = dto.Description,
            Attachments = dto.Attachments,
            Status = DisputeStatusConstants.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _disputeRepo.CreateAsync(dispute);

        // Notify the other party
        await NotifyDisputeOpenedAsync(job, role);

        return MapToDetail(created, job);
    }

    public async Task<DisputeDetailDto?> GetDisputeForJobAsync(int jobId, int userId, string role)
    {
        var job = await _jobRepo.GetByIdAsync(jobId);
        if (job == null) return null;

        // Only the involved parties can view the dispute
        if (role == "customer" && job.CustomerId != userId) return null;
        if (role == "craftsman")
        {
            var craftsman = await _craftsmanRepo.GetByUserIdAsync(userId);
            if (craftsman == null || job.CraftsmanId != craftsman.Id) return null;
        }

        var dispute = await _disputeRepo.GetActiveDisputeForJobAsync(jobId);
        if (dispute == null) return null;

        return MapToDetail(dispute, job);
    }

    public async Task<IEnumerable<DisputeSummaryDto>> GetMyDisputesAsync(int userId)
    {
        var disputes = await _disputeRepo.GetByUserIdAsync(userId);
        return disputes.Select(d => new DisputeSummaryDto
        {
            Id = d.Id,
            JobId = d.JobId,
            Status = d.Status,
            RaisedByRole = d.RaisedByRole,
            Reason = d.Reason,
            CreatedAt = d.CreatedAt,
            ResolvedAt = d.ResolvedAt,
            FavoredParty = d.FavoredParty
        });
    }

    public async Task<DisputeDetailDto> RespondToDisputeAsync(int disputeId, int userId, string role, DisputeResponseRequest dto)
    {
        var dispute = await _disputeRepo.GetByIdAsync(disputeId)
            ?? throw new KeyNotFoundException("النزاع غير موجود.");

        // Only the non-initiating party can respond
        if (dispute.RaisedByUserId == userId)
            throw new UnauthorizedAccessException("لا يمكنك الرد على نزاع قمت أنت بفتحه.");

        // Verify the user is a party to this dispute
        var job = dispute.Job;
        if (role == "customer" && job.CustomerId != userId)
            throw new UnauthorizedAccessException("لا يمكنك الرد على هذا النزاع.");
        if (role == "craftsman")
        {
            var craftsman = await _craftsmanRepo.GetByUserIdAsync(userId);
            if (craftsman == null || job.CraftsmanId != craftsman.Id)
                throw new UnauthorizedAccessException("لا يمكنك الرد على هذا النزاع.");
        }

        // Only active disputes can be responded to
        if (!DisputeStatusConstants.Active.Contains(dispute.Status))
            throw new InvalidOperationException("لا يمكن الرد على نزاع تم حله أو رفضه بالفعل.");

        dispute.ResponseMessage = dto.Message;
        dispute.ResponseAttachments = dto.Attachments;
        dispute.Status = DisputeStatusConstants.UnderReview;

        var updated = await _disputeRepo.UpdateAsync(dispute);

        return MapToDetail(updated, job);
    }

    // ── Notifications ──────────────────────────────────────────

    private async Task NotifyDisputeOpenedAsync(Job job, string openedByRole)
    {
        var receiverId = openedByRole == "customer"
            ? job.Craftsman?.UserId
            : job.CustomerId;

        if (receiverId.HasValue)
        {
            var notifDto = await _notificationService.CreateJobNotificationAsync(
                receiverId.Value,
                "تم فتح نزاع",
                "تم فتح نزاع على طلب الخدمة الخاص بك.",
                "dispute_opened",
                job.Id);

            await _notifPusher.PushAsync(receiverId.Value, notifDto);
        }
    }

    // ── Mapping ────────────────────────────────────────────────

    private static DisputeDetailDto MapToDetail(Dispute d, Job job)
    {
        var conv = job.Conversation;

        return new DisputeDetailDto
        {
            Id = d.Id,
            JobId = d.JobId,
            JobServiceType = job.ServiceType,
            JobStatus = job.Status,
            JobDescription = job.Description,

            RaisedByUserId = d.RaisedByUserId,
            RaisedByRole = d.RaisedByRole,

            Status = d.Status,
            Reason = d.Reason,
            Description = d.Description,
            Attachments = d.Attachments,
            CreatedAt = d.CreatedAt,
            ResolvedAt = d.ResolvedAt,

            ResponseMessage = d.ResponseMessage,
            ResponseAttachments = d.ResponseAttachments,

            Resolution = d.Resolution,
            FavoredParty = d.FavoredParty,
            ResolvedByAdminId = d.ResolvedByAdminId,

            CustomerId = job.CustomerId,
            CustomerName = job.Customer?.Name ?? string.Empty,
            CraftsmanId = job.CraftsmanId,
            CraftsmanName = job.Craftsman?.User?.Name,

            ConversationId = conv?.Id,
            HasActiveDispute = DisputeStatusConstants.Active.Contains(d.Status)
        };
    }
}
