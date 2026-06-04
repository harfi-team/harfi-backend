using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("pending-craftsmen")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _adminService.GetPendingCraftsmenAsync();
            return Ok(pending);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _adminService.ApproveCraftsmanAsync(id);
            if (!result) return NotFound("The Craftsman is not Present");
            return Ok("The craftsman's application was approved and his account successfully activated.");
        }

        [HttpDelete("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _adminService.RejectCraftsmanAsync(id);
            if (!result) return NotFound("The Craftsman is not Present");
            return Ok("The craftsman's request was rejected and deleted from the system.");
        }
    }
}