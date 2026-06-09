using Harfi.DTOs.Job;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harfi.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
    {
        var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _jobService.CreateJobAsync(customerId, dto);
        return CreatedAtAction(nameof(CreateJob), result);
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
        }
        else
        {
            return Forbid();
        }

        var result = await _jobService.GetCraftsmanJobsAsync(id);
        return Ok(result);
    }
}