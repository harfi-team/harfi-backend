using System.Security.Claims;
using Harfi.DTOs.Admin;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harfi.API.Controllers;

[Route("api/v1/admin")]
[ApiController]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private int GetAdminId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string? GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    // ═══════════════════════════════════════════════════════════
    //  CRAFTSMAN VERIFICATION
    // ═══════════════════════════════════════════════════════════

    [HttpGet("craftsmen/pending")]
    public async Task<IActionResult> GetPendingCraftsmen(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? city = null, [FromQuery] string? serviceType = null)
    {
        var result = await _adminService.GetPendingCraftsmenAsync(page, pageSize, city, serviceType);
        return Ok(result);
    }

    [HttpGet("craftsmen/approved")]
    public async Task<IActionResult> GetApprovedCraftsmen(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? city = null, [FromQuery] string? serviceType = null,
        [FromQuery] decimal? minRating = null)
    {
        var result = await _adminService.GetApprovedCraftsmenAsync(page, pageSize, city, serviceType, minRating);
        return Ok(result);
    }

    [HttpGet("craftsmen/rejected")]
    public async Task<IActionResult> GetRejectedCraftsmen(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetRejectedCraftsmenAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("craftsmen/{id}")]
    public async Task<IActionResult> GetCraftsmanById(int id)
    {
        try
        {
            var result = await _adminService.GetCraftsmanByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("craftsmen/{id}/approve")]
    public async Task<IActionResult> ApproveCraftsman(int id, [FromBody] ApproveCraftsmanRequest? request)
    {
        var result = await _adminService.ApproveCraftsmanAsync(id, request?.NotifyMessage, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpPut("craftsmen/{id}/reject")]
    public async Task<IActionResult> RejectCraftsman(int id, [FromBody] RejectCraftsmanRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.RejectCraftsmanAsync(id, request.Reason, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpPut("craftsmen/{id}/suspend")]
    public async Task<IActionResult> SuspendCraftsman(int id, [FromBody] SuspendCraftsmanRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.SuspendCraftsmanAsync(id, request.Reason, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpDelete("craftsmen/{id}")]
    public async Task<IActionResult> DeleteCraftsman(int id, [FromBody] DeleteCraftsmanRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.SoftDeleteCraftsmanAsync(id, request.Reason, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  USER MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] bool? isActive = null, [FromQuery] string? search = null)
    {
        var result = await _adminService.GetUsersAsync(role, page, pageSize, isActive, search);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var result = await _adminService.GetUserByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("users/{id}/activity")]
    public async Task<IActionResult> GetUserActivity(int id)
    {
        try
        {
            var result = await _adminService.GetUserActivityAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id, [FromBody] DeactivateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _adminService.DeactivateUserAsync(id, request.Reason, GetAdminId(), GetIpAddress());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id}/reactivate")]
    public async Task<IActionResult> ReactivateUser(int id)
    {
        try
        {
            var result = await _adminService.ReactivateUserAsync(id, GetAdminId(), GetIpAddress());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id, [FromBody] DeleteUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _adminService.SoftDeleteUserAsync(id, request.Reason, GetAdminId(), GetIpAddress());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  JOBS & DISPUTES
    // ═══════════════════════════════════════════════════════════

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? status = null, [FromQuery] int? craftsmanId = null,
        [FromQuery] int? customerId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _adminService.GetJobsAsync(status, craftsmanId, customerId, page, pageSize, from, to);
        return Ok(result);
    }

    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> GetJobById(int id)
    {
        try
        {
            var result = await _adminService.GetJobByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("jobs/{id}/status")]
    public async Task<IActionResult> UpdateJobStatus(int id, [FromBody] UpdateJobStatusRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.UpdateJobStatusAsync(id, request.Status, request.Justification, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpPut("jobs/{id}/flag-dispute")]
    public async Task<IActionResult> FlagDispute(int id, [FromBody] FlagDisputeRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.FlagDisputeAsync(id, request.Reason, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpPut("jobs/{id}/resolve-dispute")]
    public async Task<IActionResult> ResolveDispute(int id, [FromBody] ResolveDisputeRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.ResolveDisputeAsync(id, request.Resolution, request.FavoredParty, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpGet("jobs/{id}/chat-metadata")]
    public async Task<IActionResult> GetJobChatMetadata(int id)
    {
        try
        {
            var result = await _adminService.GetJobChatMetadataAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("jobs/{id}/chat-messages")]
    public async Task<IActionResult> GetJobChatMessages(int id)
    {
        try
        {
            var result = await _adminService.GetJobMessagesForAdminAsync(id, GetAdminId(), GetIpAddress());
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CONTENT MODERATION
    // ═══════════════════════════════════════════════════════════

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int? craftsmanId = null, [FromQuery] int? minStars = null,
        [FromQuery] int? maxStars = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetReviewsAsync(craftsmanId, minStars, maxStars, page, pageSize);
        return Ok(result);
    }

    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        try
        {
            var result = await _adminService.GetReviewByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(int id, [FromBody] DeleteReviewRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.SoftDeleteReviewAsync(id, request.Reason, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? status = null, [FromQuery] string? type = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetReportsAsync(status, type, page, pageSize);
        return Ok(result);
    }

    [HttpPut("reports/{id}/resolve")]
    public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.ResolveReportAsync(id, request.Action, request.Notes, GetAdminId(), GetIpAddress());
        return Ok(result);
    }

    [HttpGet("ai-logs")]
    public async Task<IActionResult> GetAiLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _adminService.GetAiLogsAsync(page, pageSize, from, to);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  ANALYTICS
    // ═══════════════════════════════════════════════════════════

    [HttpGet("analytics/overview")]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _adminService.GetOverviewAsync();
        return Ok(result);
    }

    [HttpGet("analytics/craftsmen")]
    public async Task<IActionResult> GetCraftsmanAnalytics()
    {
        var result = await _adminService.GetCraftsmanAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("analytics/jobs")]
    public async Task<IActionResult> GetJobAnalytics()
    {
        var result = await _adminService.GetJobAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("analytics/ai")]
    public async Task<IActionResult> GetAiAnalytics()
    {
        var result = await _adminService.GetAiAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("analytics/reviews")]
    public async Task<IActionResult> GetReviewAnalytics()
    {
        var result = await _adminService.GetReviewAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("analytics/export")]
    public async Task<IActionResult> ExportData(
        [FromQuery] string type = "users",
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        try
        {
            var data = await _adminService.ExportDataAsync(type, from, to);
            return File(data, "text/csv", $"harfi-{type}-{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PLATFORM CONFIG
    // ═══════════════════════════════════════════════════════════

    [HttpGet("config/service-types")]
    public async Task<IActionResult> GetServiceTypes()
    {
        var result = await _adminService.GetServiceTypesAsync();
        return Ok(result);
    }

    [HttpGet("config/cities")]
    public async Task<IActionResult> GetCities()
    {
        var result = await _adminService.GetCitiesAsync();
        return Ok(result);
    }

    [HttpGet("config/feature-flags")]
    public async Task<IActionResult> GetFeatureFlags()
    {
        var result = await _adminService.GetFeatureFlagsAsync();
        return Ok(result);
    }

    [HttpPut("config/feature-flags/{key}")]
    public async Task<IActionResult> UpdateFeatureFlag(string key, [FromBody] UpdateFeatureFlagRequest request)
    {
        var result = await _adminService.UpdateFeatureFlagAsync(key, request.IsEnabled);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  AUDIT LOGS
    // ═══════════════════════════════════════════════════════════

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int? adminId = null, [FromQuery] string? action = null,
        [FromQuery] string? targetType = null, [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetAuditLogsAsync(adminId, action, targetType, from, to, page, pageSize);
        return Ok(result);
    }

    [HttpGet("audit-logs/{id}")]
    public async Task<IActionResult> GetAuditLogById(int id)
    {
        try
        {
            var result = await _adminService.GetAuditLogByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
