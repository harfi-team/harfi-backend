using Harfi.DTOs.Job;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harfi.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IImageservice _imageService;

    public JobsController(IJobService jobService, IImageservice imageService)
    {
        _jobService = jobService;
        _imageService = imageService;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
    {
        var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _jobService.CreateJobAsync(customerId, dto);
        return CreatedAtAction(nameof(CreateJob), result);
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> UploadJobImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "يرجى اختيار صورة للرفع." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "نوع الملف غير مدعوم. الأنواع المسموحة: JPG, PNG, WEBP." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "حجم الصورة يجب أن لا يتجاوز 5 ميجابايت." });

        var url = await _imageService.SaveImageAsync(file, "jobs");
        return Ok(new { url });
    }

    [HttpPut("{id}/accept")]
    [Authorize(Roles = "craftsman")]
    public async Task<IActionResult> AcceptJob(int id)
    {
        var craftsmanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _jobService.AcceptJobAsync(id, craftsmanId);
        return Ok(result);
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "craftsman")]
    public async Task<IActionResult> RejectJob(int id)
    {
        var craftsmanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _jobService.RejectJobAsync(id, craftsmanId);
        return Ok(result);
    }

    [HttpPut("{id}/complete")]
    [Authorize(Roles = "craftsman")]
    public async Task<IActionResult> CompleteJob(int id, [FromBody] UpdateJobStatusDto dto)
    {
        var craftsmanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _jobService.CompleteJobAsync(id, craftsmanId, dto);
        return Ok(result);
    }

    [HttpGet("customer/{id}")]
    public async Task<IActionResult> GetCustomerJobs(int id)
    {
        var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requestingRole = User.FindFirstValue(ClaimTypes.Role);
        if (requestingRole != "admin" && requestingUserId != id)
            return Forbid();

        var result = await _jobService.GetCustomerJobsAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var result = await _jobService.GetJobByIdAsync(id, userId, role);
        if (result == null)
            return Forbid();
        return Ok(result);
    }

    //[HttpGet("craftsman/{id}")]
    //public async Task<IActionResult> GetCraftsmanJobs(int id)
    //{
    //    var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    //    var requestingRole = User.FindFirstValue(ClaimTypes.Role);

    //    if (requestingRole == "admin")
    //        return Ok(await _jobService.GetCraftsmanJobsAsync(id));

    //    if (requestingRole == "craftsman")
    //    {
    //        var owns = await _jobService.CraftsmanBelongsToUserAsync(id, requestingUserId);
    //        if (!owns) return Forbid();
    //    }
    //    else
    //    {
    //        return Forbid();
    //    }

    //    var result = await _jobService.GetCraftsmanJobsAsync(id);
    //    return Ok(result);
    //}
    // ========================================
    // الكود الجديد (يسمح للـ customer):
    // ========================================

    [HttpGet("craftsman/{id}")]
    public async Task<IActionResult> GetCraftsmanJobs(int id)
    {
        var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requestingRole = User.FindFirstValue(ClaimTypes.Role);

        if (requestingRole == "admin")
            return Ok(await _jobService.GetCraftsmanJobsAsync(id));

        if (requestingRole == "craftsman")
        {
            var owns = await _jobService.CraftsmanBelongsToUserAsync(id, requestingUserId);
            if (!owns) return Forbid();
            // craftsman يشوف أعماله هو فقط
        }
        else if (requestingRole == "customer")
        {
            // customer يشوف أعمال أي حرفي (قراءة فقط) — لعرض الـ portfolio
            // لا توجد قيود على الـ customer
        }
        else
        {
            return Forbid();
        }

        var result = await _jobService.GetCraftsmanJobsAsync(id);
        return Ok(result);
    }
}