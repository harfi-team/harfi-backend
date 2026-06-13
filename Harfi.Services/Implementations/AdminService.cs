using Harfi.DTOs.Admin;
using Harfi.DTOs.Chat;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly ICraftsmanRepository _craftsmanRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Job> _jobRepo;
    private readonly IGenericRepository<Review> _reviewRepo;
    private readonly IGenericRepository<Notification> _notifRepo;
    private readonly IGenericRepository<AdminAuditLog> _auditLogRepo;
    private readonly IGenericRepository<Report> _reportRepo;
    private readonly IGenericRepository<ServiceType> _serviceTypeRepo;
    private readonly IGenericRepository<City> _cityRepo;
    private readonly IGenericRepository<FeatureFlag> _featureFlagRepo;
    private readonly IGenericRepository<AIChatMessage> _aiChatRepo;
    private readonly IGenericRepository<Message> _msgRepo;
    private readonly IConversationRepository _convRepo;
    private readonly IAuditLogService _auditLogService;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;

    public AdminService(
        ICraftsmanRepository craftsmanRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<Job> jobRepo,
        IGenericRepository<Review> reviewRepo,
        IGenericRepository<Notification> notifRepo,
        IGenericRepository<AdminAuditLog> auditLogRepo,
        IGenericRepository<Report> reportRepo,
        IGenericRepository<ServiceType> serviceTypeRepo,
        IGenericRepository<City> cityRepo,
        IGenericRepository<FeatureFlag> featureFlagRepo,
        IGenericRepository<AIChatMessage> aiChatRepo,
        IGenericRepository<Message> msgRepo,
        IConversationRepository convRepo,
        IAuditLogService auditLogService,
        UserManager<User> userManager,
        AppDbContext db)
    {
        _craftsmanRepo = craftsmanRepo;
        _userRepo = userRepo;
        _jobRepo = jobRepo;
        _reviewRepo = reviewRepo;
        _notifRepo = notifRepo;
        _auditLogRepo = auditLogRepo;
        _reportRepo = reportRepo;
        _serviceTypeRepo = serviceTypeRepo;
        _cityRepo = cityRepo;
        _featureFlagRepo = featureFlagRepo;
        _aiChatRepo = aiChatRepo;
        _msgRepo = msgRepo;
        _convRepo = convRepo;
        _auditLogService = auditLogService;
        _userManager = userManager;
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════
    //  CRAFTSMAN VERIFICATION
    // ═══════════════════════════════════════════════════════════

    public async Task<PagedResult<PendingCraftsmanDto>> GetPendingCraftsmenAsync(
        int page, int pageSize, string? city, string? serviceType)
    {
        // Admin context: must include soft-deleted users' craftsmen for pending-list review
        var query = _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .Include(c => c.Service)
            .Where(c => !c.IsApproved && !c.IsDeleted);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(c => c.CityNavigation != null &&
                (c.CityNavigation.NameAr.Contains(city) || c.CityNavigation.NameEn.Contains(city)));
        if (!string.IsNullOrEmpty(serviceType))
            query = query.Where(c => c.Service != null && (c.Service.NameAr.Contains(serviceType) || c.Service.NameEn.Contains(serviceType)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PendingCraftsmanDto>
        {
            Items = items.Select(c => new PendingCraftsmanDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User?.Name ?? string.Empty,
                Email = c.User?.Email ?? string.Empty,
                Phone = c.User?.Phone,
                ServiceType = c.Service?.NameAr ?? "غير محدد",
                CityNameAr = c.CityNavigation?.NameAr,
                CityNameEn = c.CityNavigation?.NameEn,
                Neighborhood = c.Neighborhood,
                Experience = c.Experience,
                NationalIdUrl = c.NationalIdUrl,
                Bio = c.Bio,
                CreatedAt = c.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ApprovedCraftsmanDto>> GetApprovedCraftsmenAsync(
        int page, int pageSize, string? city, string? serviceType, decimal? minRating)
    {
        // Admin context: must include soft-deleted users' craftsmen for approved-list review
        var query = _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .Include(c => c.Service)
            .Where(c => c.IsApproved && !c.IsDeleted);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(c => c.CityNavigation != null &&
                (c.CityNavigation.NameAr.Contains(city) || c.CityNavigation.NameEn.Contains(city)));
        if (!string.IsNullOrEmpty(serviceType))
            query = query.Where(c => c.Service != null && (c.Service.NameAr.Contains(serviceType) || c.Service.NameEn.Contains(serviceType)));
        if (minRating.HasValue)
            query = query.Where(c => c.Rating >= minRating.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.Rating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ApprovedCraftsmanDto>
        {
            Items = items.Select(c => new ApprovedCraftsmanDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User?.Name ?? string.Empty,
                Email = c.User?.Email ?? string.Empty,
                Phone = c.User?.Phone,
                ServiceType = c.Service?.NameAr ?? "غير محدد",
                CityNameAr = c.CityNavigation?.NameAr,
                CityNameEn = c.CityNavigation?.NameEn,
                Neighborhood = c.Neighborhood,
                Experience = c.Experience,
                Rating = c.Rating,
                IsAvailable = c.IsAvailable,
                Bio = c.Bio,
                CreatedAt = c.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<RejectedCraftsmanDto>> GetRejectedCraftsmenAsync(int page, int pageSize)
    {
        // Admin context: must bypass !c.IsDeleted filter to find rejected (soft-deleted) craftsmen
        var query = _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .Include(c => c.Service)
            .Where(c => c.IsDeleted);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<RejectedCraftsmanDto>
        {
            Items = items.Select(c => new RejectedCraftsmanDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User?.Name ?? string.Empty,
                Email = c.User?.Email ?? string.Empty,
                Phone = c.User?.Phone,
                ServiceType = c.Service?.NameAr ?? "غير محدد",
                CityNameAr = c.CityNavigation?.NameAr,
                CityNameEn = c.CityNavigation?.NameEn,
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt,
                DeletedAt = c.DeletedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CraftsmanDetailDto> GetCraftsmanByIdAsync(int id)
    {
        // Admin context: must include soft-deleted craftsmen and their related data for investigation
        var craftsman = await _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .Include(c => c.Service)
            .Include(c => c.Jobs)
            .Include(c => c.Reviews)
            .Include(c => c.CityNavigation)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("الحرفي غير موجود.");

        return new CraftsmanDetailDto
        {
            Id = craftsman.Id,
            UserId = craftsman.UserId,
            FullName = craftsman.User?.Name ?? string.Empty,
            Email = craftsman.User?.Email ?? string.Empty,
            Phone = craftsman.User?.Phone,
            ProfileImageUrl = craftsman.User?.ProfileImageUrl,
            ServiceType = craftsman.Service?.NameAr ?? "غير محدد",
            CityNameAr = craftsman.CityNavigation?.NameAr,
            CityNameEn = craftsman.CityNavigation?.NameEn,
            Neighborhood = craftsman.Neighborhood,
            PriceRangeMin = craftsman.PriceRangeMin,
            PriceRangeMax = craftsman.PriceRangeMax,
            Experience = craftsman.Experience,
            IsApproved = craftsman.IsApproved,
            IsAvailable = craftsman.IsAvailable,
            IsDeleted = craftsman.IsDeleted,
            Rating = craftsman.Rating,
            Bio = craftsman.Bio,
            NationalIdUrl = craftsman.NationalIdUrl,
            RejectionReason = craftsman.RejectionReason,
            DeletionReason = craftsman.DeletionReason,
            CreatedAt = craftsman.CreatedAt,
            UpdatedAt = craftsman.UpdatedAt,
            CompletedJobsCount = craftsman.Jobs.Count(j => j.Status == JobStatusConstants.Done),
            TotalReviews = craftsman.Reviews.Count
        };
    }

    public async Task<AdminActionResponse> ApproveCraftsmanAsync(
        int id, string? notifyMessage, int adminId, string? ipAddress)
    {
        // Admin context: must find craftsman by ID regardless of soft-delete status
        var craftsman = await _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("الحرفي غير موجود.");

        if (craftsman.IsApproved)
            return AdminActionResponse.Fail("الحرفي معتمد بالفعل.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            craftsman.IsApproved = true;
            craftsman.UpdatedAt = DateTime.UtcNow;
            await _craftsmanRepo.UpdateAsync(craftsman);

            var message = notifyMessage ?? "تم اعتماد طلبك بنجاح! يمكنك الآن استقبال طلبات العملاء.";
            await SendNotificationAsync(craftsman.UserId, "تم الاعتماد", message, "approved", null);

            await _auditLogService.LogAsync(adminId, "approve_craftsman", "Craftsman", id,
                $"Approved craftsman {craftsman.User?.Name}", ipAddress);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return AdminActionResponse.Ok("تم اعتماد الحرفي بنجاح.");
    }

    public async Task<AdminActionResponse> RejectCraftsmanAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        // Admin context: must find craftsman by ID regardless of soft-delete status
        var craftsman = await _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("الحرفي غير موجود.");

        craftsman.RejectionReason = reason;
        craftsman.IsDeleted = true;
        craftsman.DeletedAt = DateTime.UtcNow;
        craftsman.DeletedByAdminId = adminId;
        craftsman.DeletionReason = reason;
        craftsman.UpdatedAt = DateTime.UtcNow;
        await _craftsmanRepo.UpdateAsync(craftsman);

        var message = $"تم رفض طلب التسجيل الخاص بك. السبب: {reason}";
        await SendNotificationAsync(craftsman.UserId, "تم الرفض", message, "rejected", null);

        await _auditLogService.LogAsync(adminId, "reject_craftsman", "Craftsman", id,
            $"Rejected craftsman {craftsman.User?.Name}. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم رفض الحرفي.");
    }

    public async Task<AdminActionResponse> SuspendCraftsmanAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        // Admin context: must find craftsman by ID regardless of soft-delete status
        var craftsman = await _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("الحرفي غير موجود.");

        craftsman.IsAvailable = false;
        craftsman.UpdatedAt = DateTime.UtcNow;
        await _craftsmanRepo.UpdateAsync(craftsman);

        var message = $"تم تعليق حسابك. السبب: {reason}";
        await SendNotificationAsync(craftsman.UserId, "تم التعليق", message, "suspended", null);

        await _auditLogService.LogAsync(adminId, "suspend_craftsman", "Craftsman", id,
            $"Suspended craftsman {craftsman.User?.Name}. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم تعليق الحرفي.");
    }

    public async Task<AdminActionResponse> SoftDeleteCraftsmanAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        // Admin context: must find craftsman by ID regardless of soft-delete status
        var craftsman = await _craftsmanRepo.GetAllWithUserQuery().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("الحرفي غير موجود.");

        craftsman.IsDeleted = true;
        craftsman.DeletedAt = DateTime.UtcNow;
        craftsman.DeletedByAdminId = adminId;
        craftsman.DeletionReason = reason;
        craftsman.IsAvailable = false;
        craftsman.UpdatedAt = DateTime.UtcNow;
        await _craftsmanRepo.UpdateAsync(craftsman);

        await _auditLogService.LogAsync(adminId, "delete_craftsman", "Craftsman", id,
            $"Soft-deleted craftsman {craftsman.User?.Name}. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم حذف الحرفي.");
    }

    // ═══════════════════════════════════════════════════════════
    //  USER MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    public async Task<PagedResult<UserAdminDto>> GetUsersAsync(
        string? role, int page, int pageSize, bool? isActive, string? search)
    {
        // Admin context: must include soft-deleted users for account management
        var query = _userRepo.GetQueryable().IgnoreQueryFilters();

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role == role);
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Name.Contains(search) || (u.Email != null && u.Email.Contains(search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserAdminDto>
        {
            Items = items.Select(u => new UserAdminDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email ?? string.Empty,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                IsVerified = u.IsVerified,
                IsDeleted = u.IsDeleted,
                ProfileImageUrl = u.ProfileImageUrl,
                CreatedAt = u.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserAdminDetailDto> GetUserByIdAsync(int id)
    {
        // Admin context: must include soft-deleted users for investigation
        var user = await _userManager.Users.IgnoreQueryFilters()
            .Include(u => u.CraftsmanProfile)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        return new UserAdminDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            IsDeleted = user.IsDeleted,
            ProfileImageUrl = user.ProfileImageUrl,
            DeletionReason = user.DeletionReason,
            DeletedAt = user.DeletedAt,
            CreatedAt = user.CreatedAt,
            CraftsmanProfileId = user.CraftsmanProfile?.Id ?? 0
        };
    }

    public async Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(int id)
    {
        // Admin context: must include soft-deleted users for audit trail
        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        var activities = new List<UserActivityDto>();

        var logs = await _auditLogRepo.FindAsync(l => l.TargetType == "User" && l.TargetId == id);
        foreach (var log in logs.OrderByDescending(l => l.CreatedAt).Take(50))
        {
            activities.Add(new UserActivityDto
            {
                Action = log.Action,
                Details = log.Notes ?? string.Empty,
                Timestamp = log.CreatedAt
            });
        }

        return activities;
    }

    public async Task<AdminActionResponse> DeactivateUserAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        // Admin context: must find user by ID regardless of soft-delete status
        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.Role == "admin")
            throw new InvalidOperationException("لا يمكن تعطيل حساب أدمن آخر.");
        if (user.Id == adminId)
            throw new InvalidOperationException("لا يمكن تعطيل حسابك الخاص.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        await _auditLogService.LogAsync(adminId, "deactivate_user", "User", id,
            $"Deactivated user {user.Name}. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم تعطيل المستخدم.");
    }

    public async Task<AdminActionResponse> ReactivateUserAsync(
        int id, int adminId, string? ipAddress)
    {
        // Admin context: must find user by ID regardless of soft-delete status
        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        await _auditLogService.LogAsync(adminId, "reactivate_user", "User", id,
            $"Reactivated user {user.Name}", ipAddress);

        return AdminActionResponse.Ok("تم إعادة تفعيل المستخدم.");
    }

    public async Task<AdminActionResponse> SoftDeleteUserAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        // Admin context: must find user by ID regardless of soft-delete status
        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.Role == "admin")
            throw new InvalidOperationException("لا يمكن حذف أدمن آخر.");
        if (user.Id == adminId)
            throw new InvalidOperationException("لا يمكن حذف حسابك الخاص.");

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedByAdminId = adminId;
        user.DeletionReason = reason;
        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        await _auditLogService.LogAsync(adminId, "delete_user", "User", id,
            $"Soft-deleted user {user.Name}. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم حذف المستخدم.");
    }

    // ═══════════════════════════════════════════════════════════
    //  JOBS & DISPUTES
    // ═══════════════════════════════════════════════════════════

    public async Task<PagedResult<JobAdminDto>> GetJobsAsync(
        string? status, int? craftsmanId, int? customerId, int page, int pageSize, DateTime? from, DateTime? to)
    {
        // Admin context: must include soft-deleted customers/craftsmen for job management
        IQueryable<Job> query = _jobRepo.GetQueryable().IgnoreQueryFilters()
            .Include(j => j.Customer)
            .Include(j => j.Craftsman!).ThenInclude(c => c.User);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(j => j.Status == status);
        if (craftsmanId.HasValue)
            query = query.Where(j => j.CraftsmanId == craftsmanId);
        if (customerId.HasValue)
            query = query.Where(j => j.CustomerId == customerId);
        if (from.HasValue)
            query = query.Where(j => j.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(j => j.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<JobAdminDto>
        {
            Items = items.Select(j => new JobAdminDto
            {
                Id = j.Id,
                CustomerId = j.CustomerId,
                CustomerName = j.Customer?.Name ?? string.Empty,
                CraftsmanId = j.CraftsmanId,
                CraftsmanName = j.Craftsman?.User?.Name,
                Status = j.Status,
                ServiceType = j.ServiceType,
                Description = j.Description,
                Address = j.Address,
                IsDisputed = j.IsDisputed,
                CreatedAt = j.CreatedAt,
                CompletedAt = j.CompletedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<JobDetailDto> GetJobByIdAsync(int id)
    {
        // Admin context: must include soft-deleted customers/craftsmen for job detail view
        var job = await _jobRepo.GetQueryable().IgnoreQueryFilters()
            .Include(j => j.Customer)
            .Include(j => j.Craftsman!).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        return new JobDetailDto
        {
            Id = job.Id,
            CustomerId = job.CustomerId,
            CustomerName = job.Customer?.Name ?? string.Empty,
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
            IsDisputed = job.IsDisputed,
            DisputeRaisedAt = job.DisputeRaisedAt,
            DisputeResolvedAt = job.DisputeResolvedAt,
            DisputeResolution = job.DisputeResolution,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            UpdatedAt = job.UpdatedAt
        };
    }

    public async Task<AdminActionResponse> UpdateJobStatusAsync(
        int id, string status, string justification, int adminId, string? ipAddress)
    {
        var job = await _jobRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        job.Status = status;
        job.UpdatedAt = DateTime.UtcNow;
        _jobRepo.Update(job);
        await _jobRepo.SaveChangesAsync();

        await _auditLogService.LogAsync(adminId, "update_job_status", "Job", id,
            $"Changed status to {status}. Justification: {justification}", ipAddress);

        return AdminActionResponse.Ok("تم تحديث حالة الوظيفة.");
    }

    public async Task<AdminActionResponse> FlagDisputeAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        var job = await _jobRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        job.IsDisputed = true;
        job.DisputeRaisedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        _jobRepo.Update(job);
        await _jobRepo.SaveChangesAsync();

        await _auditLogService.LogAsync(adminId, "flag_dispute", "Job", id,
            $"Flagged dispute. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم الإبلاغ عن النزاع.");
    }

    public async Task<AdminActionResponse> ResolveDisputeAsync(
        int id, string resolution, string favoredParty, int adminId, string? ipAddress)
    {
        var job = await _jobRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        if (!job.IsDisputed)
            return AdminActionResponse.Fail("الوظيفة ليس بها نزاع.");

        job.IsDisputed = false;
        job.DisputeResolvedAt = DateTime.UtcNow;
        job.DisputeResolution = $"Resolution: {resolution}. Favored: {favoredParty}";
        job.UpdatedAt = DateTime.UtcNow;
        _jobRepo.Update(job);
        await _jobRepo.SaveChangesAsync();

        await _auditLogService.LogAsync(adminId, "resolve_dispute", "Job", id,
            $"Resolved dispute. Resolution: {resolution}. Favored: {favoredParty}", ipAddress);

        return AdminActionResponse.Ok("تم حل النزاع.");
    }

    public async Task<ChatMetadataDto> GetJobChatMetadataAsync(int id)
    {
        // Admin context: must bypass Conversation/Customer/Craftsman filters for dispute resolution
        var job = await _jobRepo.GetQueryable().IgnoreQueryFilters()
            .Include(j => j.Customer)
            .Include(j => j.Craftsman!).ThenInclude(c => c.User)
            .Include(j => j.Conversation)
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        if (job.Conversation == null)
            throw new KeyNotFoundException("لا توجد محادثة لهذه الوظيفة.");

        return new ChatMetadataDto
        {
            ConversationId = job.Conversation.Id,
            JobId = job.Id,
            CustomerName = job.Customer?.Name ?? string.Empty,
            CraftsmanName = job.Craftsman?.User?.Name ?? string.Empty,
            MessageCount = job.Conversation.Messages?.Count ?? 0,
            CreatedAt = job.Conversation.CreatedAt,
            LastMessageAt = job.Conversation.LastMessageAt
        };
    }

    public async Task<IEnumerable<MessageDto>> GetJobMessagesForAdminAsync(
        int jobId, int adminId, string? ipAddress)
    {
        // Admin context: must bypass Conversation filter to access disputed job messages
        var job = await _jobRepo.GetQueryable().IgnoreQueryFilters()
            .Include(j => j.Conversation)
            .FirstOrDefaultAsync(j => j.Id == jobId)
            ?? throw new KeyNotFoundException("الوظيفة غير موجودة.");

        if (!job.IsDisputed)
            throw new UnauthorizedAccessException("لا يمكن الوصول إلى رسائل المحادثة إلا في حالة وجود نزاع.");

        if (job.Conversation == null)
            return Enumerable.Empty<MessageDto>();

        // Admin context: must include soft-deleted message senders for dispute investigation
        var messages = await _msgRepo.GetQueryable().IgnoreQueryFilters()
            .Where(m => m.ConversationId == job.Conversation.Id)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        await _auditLogService.LogAsync(adminId, "view_dispute_messages", "Job", jobId,
            "Admin accessed disputed job messages", ipAddress);

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.Name ?? string.Empty,
            SenderAvatar = m.Sender?.ProfileImageUrl,
            Content = m.Content,
            MessageType = m.MessageType,
            IsRead = m.IsRead,
            SentAt = m.SentAt
        });
    }

    // ═══════════════════════════════════════════════════════════
    //  CONTENT MODERATION
    // ═══════════════════════════════════════════════════════════

    public async Task<PagedResult<ReviewAdminDto>> GetReviewsAsync(
        int? craftsmanId, int? minStars, int? maxStars, int page, int pageSize)
    {
        // Admin context: must include soft-deleted reviews and their users for moderation
        IQueryable<Review> query = _reviewRepo.GetQueryable().IgnoreQueryFilters()
            .Include(r => r.Customer)
            .Include(r => r.Craftsman).ThenInclude(c => c.User);

        if (craftsmanId.HasValue)
            query = query.Where(r => r.CraftsmanId == craftsmanId);
        if (minStars.HasValue)
            query = query.Where(r => r.Stars >= minStars.Value);
        if (maxStars.HasValue)
            query = query.Where(r => r.Stars <= maxStars.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReviewAdminDto>
        {
            Items = items.Select(r => new ReviewAdminDto
            {
                Id = r.Id,
                JobId = r.JobId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer?.Name ?? string.Empty,
                CraftsmanId = r.CraftsmanId,
                CraftsmanName = r.Craftsman?.User?.Name ?? string.Empty,
                Stars = r.Stars,
                Comment = r.Comment,
                IsDeleted = r.IsDeleted,
                CreatedAt = r.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewAdminDetailDto> GetReviewByIdAsync(int id)
    {
        // Admin context: must include soft-deleted reviews and their users for investigation
        var review = await _reviewRepo.GetQueryable().IgnoreQueryFilters()
            .Include(r => r.Customer)
            .Include(r => r.Craftsman).ThenInclude(c => c.User)
            .Include(r => r.Job)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("التقييم غير موجود.");

        return new ReviewAdminDetailDto
        {
            Id = review.Id,
            JobId = review.JobId,
            JobDescription = review.Job?.Description ?? string.Empty,
            CustomerId = review.CustomerId,
            CustomerName = review.Customer?.Name ?? string.Empty,
            CraftsmanId = review.CraftsmanId,
            CraftsmanName = review.Craftsman?.User?.Name ?? string.Empty,
            Stars = review.Stars,
            Comment = review.Comment,
            IsDeleted = review.IsDeleted,
            DeletionReason = review.DeletionReason,
            DeletedAt = review.DeletedAt,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<AdminActionResponse> SoftDeleteReviewAsync(
        int id, string reason, int adminId, string? ipAddress)
    {
        var review = await _reviewRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("التقييم غير موجود.");

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedByAdminId = adminId;
        review.DeletionReason = reason;
        _reviewRepo.Update(review);
        await _reviewRepo.SaveChangesAsync();

        await _auditLogService.LogAsync(adminId, "delete_review", "Review", id,
            $"Soft-deleted review. Reason: {reason}", ipAddress);

        return AdminActionResponse.Ok("تم حذف التقييم.");
    }

    public async Task<PagedResult<ReportDto>> GetReportsAsync(
        string? status, string? type, int page, int pageSize)
    {
        var query = _reportRepo.GetQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(r => r.TargetType == type);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = items.Select(r => new ReportDto
            {
                Id = r.Id,
                ReportedByUserId = r.ReportedByUserId,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                Reason = r.Reason,
                Status = r.Status,
                ResolvedByAdminId = r.ResolvedByAdminId,
                ResolutionNotes = r.ResolutionNotes,
                CreatedAt = r.CreatedAt,
                ResolvedAt = r.ResolvedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminActionResponse> ResolveReportAsync(
        int id, string action, string notes, int adminId, string? ipAddress)
    {
        var report = await _reportRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("البلاغ غير موجود.");

        report.Status = "resolved";
        report.ResolvedByAdminId = adminId;
        report.ResolutionNotes = $"{notes} (Action: {action})";
        report.ResolvedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _reportRepo.SaveChangesAsync();

        await _auditLogService.LogAsync(adminId, "resolve_report", "Report", id,
            $"Resolved report. Action: {action}. Notes: {notes}", ipAddress);

        return AdminActionResponse.Ok("تم حل البلاغ.");
    }

    public async Task<PagedResult<AiLogDto>> GetAiLogsAsync(
        int page, int pageSize, DateTime? from, DateTime? to)
    {
        // Admin context: must include soft-deleted users' AI chat logs for audit
        IQueryable<AIChatMessage> query = _aiChatRepo.GetQueryable().IgnoreQueryFilters()
            .Include(a => a.User);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AiLogDto>
        {
            Items = items.Select(a => new AiLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                SessionId = a.SessionId,
                Role = a.Role,
                Content = a.Content.Length > 200 ? a.Content[..200] + "..." : a.Content,
                ToolUsed = a.ToolUsed,
                TokensUsed = a.TokensUsed,
                CreatedAt = a.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  ANALYTICS
    // ═══════════════════════════════════════════════════════════

    public async Task<AdminOverviewDto> GetOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalUsers       = await _userRepo.GetQueryable().IgnoreQueryFilters().CountAsync();
        var totalCraftsmen   = await _craftsmanRepo.GetQueryable().IgnoreQueryFilters().CountAsync();
        var pendingCraftsmen = await _craftsmanRepo.GetQueryable().IgnoreQueryFilters()
            .CountAsync(c => !c.IsApproved && !c.IsDeleted);
        var activeJobs       = await _jobRepo.GetQueryable()
            .CountAsync(j => j.Status == JobStatusConstants.Open || j.Status == JobStatusConstants.InProgress);
        var completedJobs    = await _jobRepo.GetQueryable()
            .CountAsync(j => j.Status == JobStatusConstants.Done);
        var disputedJobs     = await _jobRepo.GetQueryable()
            .CountAsync(j => j.IsDisputed);
        var pendingReports   = await _reportRepo.GetQueryable()
            .CountAsync(r => r.Status == "pending");
        var totalReviews     = await _reviewRepo.GetQueryable().IgnoreQueryFilters().CountAsync();
        var newUsers         = await _userRepo.GetQueryable().IgnoreQueryFilters()
            .CountAsync(u => u.CreatedAt >= monthStart);
        var avgRating        = await _craftsmanRepo.GetQueryable().IgnoreQueryFilters()
            .Where(c => c.Rating > 0)
            .AverageAsync(c => (double?)c.Rating) ?? 0.0;

        return new AdminOverviewDto
        {
            TotalUsers        = totalUsers,
            TotalCraftsmen    = totalCraftsmen,
            PendingCraftsmen  = pendingCraftsmen,
            ActiveJobs        = activeJobs,
            CompletedJobs     = completedJobs,
            DisputedJobs      = disputedJobs,
            PendingReports    = pendingReports,
            TotalReviews      = totalReviews,
            NewUsersThisMonth = newUsers,
            AverageRating     = Math.Round(avgRating, 2)
        };
    }

    public async Task<CraftsmanAnalyticsDto> GetCraftsmanAnalyticsAsync()
    {
        // Admin context: analytics must count all craftsmen including soft-deleted
        var all = await _craftsmanRepo.GetQueryable().IgnoreQueryFilters()
            .Include(c => c.Service)
            .ToListAsync();

        return new CraftsmanAnalyticsDto
        {
            TotalCraftsmen = all.Count,
            PendingApproval = all.Count(c => !c.IsApproved && !c.IsDeleted),
            Approved = all.Count(c => c.IsApproved && !c.IsDeleted),
            Rejected = all.Count(c => c.IsDeleted),
            Suspended = all.Count(c => !c.IsAvailable && c.IsApproved && !c.IsDeleted),
            AverageRating = all.Where(c => c.Rating > 0).Select(c => (double)(c.Rating ?? 0m)).DefaultIfEmpty(0).Average(),
            ByServiceType = all.Where(c => !c.IsDeleted).GroupBy(c => c.Service?.NameAr ?? "غير محدد")
                .ToDictionary(g => g.Key, g => g.Count()),
            ByCity = all.Where(c => !c.IsDeleted && c.CityNavigation != null).GroupBy(c => c.CityNavigation.NameAr)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<JobAnalyticsDto> GetJobAnalyticsAsync()
    {
        var jobs = (await _jobRepo.FindAsync(j => true)).ToList();

        var completionDays = jobs
            .Where(j => j.CompletedAt.HasValue && j.CreatedAt != default)
            .Select(j => (j.CompletedAt.GetValueOrDefault() - j.CreatedAt).TotalDays)
            .ToList();

        return new JobAnalyticsDto
        {
            TotalJobs = jobs.Count,
            Open = jobs.Count(j => j.Status == JobStatusConstants.Open),
            InProgress = jobs.Count(j => j.Status == JobStatusConstants.InProgress),
            Completed = jobs.Count(j => j.Status == JobStatusConstants.Done),
            Rejected = jobs.Count(j => j.Status == JobStatusConstants.Rejected),
            Disputed = jobs.Count(j => j.IsDisputed),
            ByServiceType = jobs.GroupBy(j => j.ServiceType)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageCompletionDays = completionDays.Any() ? completionDays.Average() : 0
        };
    }

    public async Task<AiAnalyticsDto> GetAiAnalyticsAsync()
    {
        var chats = (await _aiChatRepo.FindAsync(a => a.Role == "user")).ToList();

        return new AiAnalyticsDto
        {
            TotalChats = chats.Count,
            TotalTokensUsed = chats.Sum(c => c.TokensUsed ?? 0),
            TotalCraftsmenIngested = 0,
            TotalSolutionsIngested = 0,
            AverageTokensPerChat = chats.Any() ? chats.Average(c => c.TokensUsed ?? 0) : 0
        };
    }

    public async Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync()
    {
        // Admin context: analytics must count all reviews including soft-deleted
        var reviews = await _reviewRepo.GetQueryable().IgnoreQueryFilters().ToListAsync();

        var dist = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
            dist[i] = reviews.Count(r => r.Stars == i);

        return new ReviewAnalyticsDto
        {
            TotalReviews = reviews.Count,
            AverageStars = reviews.Any() ? reviews.Average(r => r.Stars) : 0,
            StarDistribution = dist,
            DeletedReviews = reviews.Count(r => r.IsDeleted)
        };
    }

    public async Task<byte[]> ExportDataAsync(string type, DateTime? from, DateTime? to)
    {
        var lines = new List<string>();

        switch (type.ToLower())
        {
            case "users":
                // Admin context: export must include all users including soft-deleted
                var users = await _userRepo.GetQueryable().IgnoreQueryFilters().ToListAsync();
                lines.Add("Id,Name,Email,Role,IsActive,CreatedAt");
                lines.AddRange(users.Select(u => $"{u.Id},{u.Name},{u.Email},{u.Role},{u.IsActive},{u.CreatedAt:O}"));
                break;
            case "craftsmen":
                // Admin context: export must include all craftsmen including soft-deleted
                var craftsmen = await _craftsmanRepo.GetAllWithUserQuery()
                    .IgnoreQueryFilters()
                    .Include(c => c.Service)
                    .ToListAsync();
                lines.Add("Id,FullName,ServiceType,City,IsApproved,Rating");
                lines.AddRange(craftsmen.Select(c => $"{c.Id},{c.User?.Name},{c.Service?.NameAr ?? "غير محدد"},{c.CityNavigation?.NameAr ?? ""},{c.IsApproved},{c.Rating}"));
                break;
            case "jobs":
                var jobs = await _jobRepo.FindAsync(j => true);
                lines.Add("Id,ServiceType,Status,CustomerId,CraftsmanId,CreatedAt");
                lines.AddRange(jobs.Select(j => $"{j.Id},{j.ServiceType},{j.Status},{j.CustomerId},{j.CraftsmanId},{j.CreatedAt:O}"));
                break;
            case "reviews":
                // Admin context: export must include all reviews including soft-deleted
                var reviews = await _reviewRepo.GetQueryable().IgnoreQueryFilters().ToListAsync();
                lines.Add("Id,Stars,Comment,CraftsmanId,CreatedAt");
                lines.AddRange(reviews.Select(r => $"{r.Id},{r.Stars},\"{r.Comment}\",{r.CraftsmanId},{r.CreatedAt:O}"));
                break;
            default:
                throw new ArgumentException("Invalid export type. Use: users, craftsmen, jobs, reviews");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
    }

    // ═══════════════════════════════════════════════════════════
    //  PLATFORM CONFIG
    // ═══════════════════════════════════════════════════════════

    public async Task<IEnumerable<ServiceTypeDto>> GetServiceTypesAsync()
    {
        var items = await _serviceTypeRepo.GetAllAsync();
        return items.Select(s => new ServiceTypeDto
        {
            Id = s.Id,
            NameAr = s.NameAr,
            NameEn = s.NameEn,
            Icon = s.Icon,
            IsActive = s.IsActive
        });
    }

    public async Task<ServiceTypeDto> CreateServiceTypeAsync(ServiceTypeDto dto)
    {
        var entity = new ServiceType
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Icon = dto.Icon,
            IsActive = true
        };
        await _serviceTypeRepo.AddAsync(entity);
        await _serviceTypeRepo.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<ServiceTypeDto> UpdateServiceTypeAsync(int id, ServiceTypeDto dto)
    {
        var entity = await _serviceTypeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("نوع الخدمة غير موجود.");

        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.Icon = dto.Icon;
        entity.IsActive = dto.IsActive;
        _serviceTypeRepo.Update(entity);
        await _serviceTypeRepo.SaveChangesAsync();

        dto.Id = id;
        return dto;
    }

    public async Task<AdminActionResponse> DeleteServiceTypeAsync(int id)
    {
        var entity = await _serviceTypeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("نوع الخدمة غير موجود.");

        // Admin context: must consider all non-deleted craftsmen including those whose User is deleted
        var activeCraftsmen = await _craftsmanRepo.GetQueryable().IgnoreQueryFilters()
            .Where(c => c.ServiceTypeId == id && !c.IsDeleted)
            .ToListAsync();

        if (activeCraftsmen.Any())
            return AdminActionResponse.Fail("لا يمكن حذف نوع الخدمة لأنه مستخدم من قبل حرفيين نشطين.");

        _serviceTypeRepo.Remove(entity);
        await _serviceTypeRepo.SaveChangesAsync();

        return AdminActionResponse.Ok("تم حذف نوع الخدمة.");
    }

    public async Task<IEnumerable<CityDto>> GetCitiesAsync()
    {
        var items = await _cityRepo.GetAllAsync();
        return items.Select(c => new CityDto
        {
            Id = c.Id,
            NameAr = c.NameAr,
            NameEn = c.NameEn,
            Governorate = c.Governorate,
            IsActive = c.IsActive
        });
    }

    public async Task<CityDto> CreateCityAsync(CityDto dto)
    {
        var entity = new City
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Governorate = dto.Governorate,
            IsActive = true
        };
        await _cityRepo.AddAsync(entity);
        await _cityRepo.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<CityDto> UpdateCityAsync(int id, CityDto dto)
    {
        var entity = await _cityRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المدينة غير موجودة.");

        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.Governorate = dto.Governorate;
        entity.IsActive = dto.IsActive;
        _cityRepo.Update(entity);
        await _cityRepo.SaveChangesAsync();

        dto.Id = id;
        return dto;
    }

    public async Task<AdminActionResponse> DeleteCityAsync(int id)
    {
        var entity = await _cityRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المدينة غير موجودة.");

        _cityRepo.Remove(entity);
        await _cityRepo.SaveChangesAsync();

        return AdminActionResponse.Ok("تم حذف المدينة.");
    }

    public async Task<IEnumerable<FeatureFlagDto>> GetFeatureFlagsAsync()
    {
        var items = await _featureFlagRepo.GetAllAsync();
        return items.Select(f => new FeatureFlagDto
        {
            Key = f.Key,
            IsEnabled = f.IsEnabled,
            UpdatedAt = f.UpdatedAt
        });
    }

    public async Task<AdminActionResponse> UpdateFeatureFlagAsync(string key, bool isEnabled)
    {
        var flag = await _featureFlagRepo.FirstOrDefaultAsync(f => f.Key == key);
        if (flag == null)
        {
            flag = new FeatureFlag { Key = key, IsEnabled = isEnabled };
            await _featureFlagRepo.AddAsync(flag);
        }
        else
        {
            flag.IsEnabled = isEnabled;
            flag.UpdatedAt = DateTime.UtcNow;
            _featureFlagRepo.Update(flag);
        }
        await _featureFlagRepo.SaveChangesAsync();

        return AdminActionResponse.Ok($"تم تحديث الميزة {key}.");
    }

    // ═══════════════════════════════════════════════════════════
    //  AUDIT LOGS
    // ═══════════════════════════════════════════════════════════

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int? adminId, string? action, string? targetType, DateTime? from, DateTime? to, int page, int pageSize)
    {
        // Admin context: must show admin name even if admin account is soft-deleted
        IQueryable<AdminAuditLog> query = _auditLogRepo.GetQueryable().Include(l => l.Admin).IgnoreQueryFilters();

        if (adminId.HasValue)
            query = query.Where(l => l.AdminId == adminId.Value);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(targetType))
            query = query.Where(l => l.TargetType == targetType);
        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(l => new AuditLogDto
            {
                Id = l.Id,
                AdminId = l.AdminId,
                AdminName = l.Admin?.Name ?? string.Empty,
                Action = l.Action,
                TargetType = l.TargetType,
                TargetId = l.TargetId,
                Notes = l.Notes,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            }),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AuditLogDto> GetAuditLogByIdAsync(int id)
    {
        // Admin context: must show admin name even if admin account is soft-deleted
        var log = await _auditLogRepo.GetQueryable()
            .Include(l => l.Admin).IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("سجل التدقيق غير موجود.");

        return new AuditLogDto
        {
            Id = log.Id,
            AdminId = log.AdminId,
            AdminName = log.Admin?.Name ?? string.Empty,
            Action = log.Action,
            TargetType = log.TargetType,
            TargetId = log.TargetId,
            Notes = log.Notes,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════

    private async Task SendNotificationAsync(int userId, string title, string body, string type, int? relatedJobId)
    {
        await _notifRepo.AddAsync(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            RelatedJobId = relatedJobId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _notifRepo.SaveChangesAsync();
    }
}
