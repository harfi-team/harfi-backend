using Harfi.DTOs.Chat;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAdminConversationService _adminConvService;

        public AdminController(
            IAdminService adminService,
            IAdminConversationService adminConvService)
        {
            _adminService = adminService;
            _adminConvService = adminConvService;
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

        [HttpGet("conversations")]
        public async Task<IActionResult> GetAllConversations([FromQuery] ConversationFilterDto filter)
        {
            var result = await _adminConvService.GetAllConversationsAsync(filter);
            return Ok(result);
        }

        [HttpGet("conversations/{id}")]
        public async Task<IActionResult> GetConversationDetails(int id)
        {
            var result = await _adminConvService.GetConversationWithMessagesAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}