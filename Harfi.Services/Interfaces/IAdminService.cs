using Harfi.DTOs.Admin;
using Harfi.DTOs.Chat;

namespace Harfi.Services.Interfaces;

public interface IAdminService
{
    // Craftsman Verification
    Task<PagedResult<PendingCraftsmanDto>> GetPendingCraftsmenAsync(int page, int pageSize, string? city, string? serviceType);
    Task<PagedResult<ApprovedCraftsmanDto>> GetApprovedCraftsmenAsync(int page, int pageSize, string? city, string? serviceType, decimal? minRating);
    Task<PagedResult<RejectedCraftsmanDto>> GetRejectedCraftsmenAsync(int page, int pageSize);
    Task<CraftsmanDetailDto> GetCraftsmanByIdAsync(int id);
    Task<AdminActionResponse> ApproveCraftsmanAsync(int id, string? notifyMessage, int adminId, string? ipAddress);
    Task<AdminActionResponse> RejectCraftsmanAsync(int id, string reason, int adminId, string? ipAddress);
    Task<AdminActionResponse> SuspendCraftsmanAsync(int id, string reason, int adminId, string? ipAddress);
    Task<AdminActionResponse> SoftDeleteCraftsmanAsync(int id, string reason, int adminId, string? ipAddress);

    // User Management
    Task<PagedResult<UserAdminDto>> GetUsersAsync(string? role, int page, int pageSize, bool? isActive, string? search);
    Task<UserAdminDetailDto> GetUserByIdAsync(int id);
    Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(int id);
    Task<AdminActionResponse> DeactivateUserAsync(int id, string reason, int adminId, string? ipAddress);
    Task<AdminActionResponse> ReactivateUserAsync(int id, int adminId, string? ipAddress);
    Task<AdminActionResponse> SoftDeleteUserAsync(int id, string reason, int adminId, string? ipAddress);

    // Jobs & Disputes
    Task<PagedResult<JobAdminDto>> GetJobsAsync(string? status, int? craftsmanId, int? customerId, int page, int pageSize, DateTime? from, DateTime? to);
    Task<JobDetailDto> GetJobByIdAsync(int id);
    Task<AdminActionResponse> UpdateJobStatusAsync(int id, string status, string justification, int adminId, string? ipAddress);
    Task<AdminActionResponse> FlagDisputeAsync(int id, string reason, int adminId, string? ipAddress);
    Task<AdminActionResponse> ResolveDisputeAsync(int id, string resolution, string favoredParty, int adminId, string? ipAddress);
    Task<ChatMetadataDto> GetJobChatMetadataAsync(int id);
    Task<IEnumerable<MessageDto>> GetJobMessagesForAdminAsync(int jobId, int adminId, string? ipAddress);

    // Content Moderation
    Task<PagedResult<ReviewAdminDto>> GetReviewsAsync(int? craftsmanId, int? minStars, int? maxStars, int page, int pageSize);
    Task<ReviewAdminDetailDto> GetReviewByIdAsync(int id);
    Task<AdminActionResponse> SoftDeleteReviewAsync(int id, string reason, int adminId, string? ipAddress);
    Task<PagedResult<ReportDto>> GetReportsAsync(string? status, string? type, int page, int pageSize);
    Task<AdminActionResponse> ResolveReportAsync(int id, string action, string notes, int adminId, string? ipAddress);
    Task<PagedResult<AiLogDto>> GetAiLogsAsync(int page, int pageSize, DateTime? from, DateTime? to);

    // Analytics
    Task<AdminOverviewDto> GetOverviewAsync();
    Task<CraftsmanAnalyticsDto> GetCraftsmanAnalyticsAsync();
    Task<JobAnalyticsDto> GetJobAnalyticsAsync();
    Task<AiAnalyticsDto> GetAiAnalyticsAsync();
    Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync();
    Task<byte[]> ExportDataAsync(string type, DateTime? from, DateTime? to);

    // Platform Config (read-only — values come from Craftsmen table)
    Task<IEnumerable<ServiceTypeDto>> GetServiceTypesAsync();
    Task<IEnumerable<CityDto>> GetCitiesAsync();
    Task<IEnumerable<FeatureFlagDto>> GetFeatureFlagsAsync();
    Task<AdminActionResponse> UpdateFeatureFlagAsync(string key, bool isEnabled);

    // Audit Logs
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int? adminId, string? action, string? targetType, DateTime? from, DateTime? to, int page, int pageSize);
    Task<AuditLogDto> GetAuditLogByIdAsync(int id);
}
