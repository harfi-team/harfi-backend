using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harfi.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationsController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _notifService
                .GetUserNotificationsAsync(GetUserId());
            return Ok(notifications);
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notifService.GetUnreadCountAsync(GetUserId());
            return Ok(new { unreadCount = count });
        }

        // PUT /api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (id <= 0) return BadRequest("معرف الإشعار غير صالح .");

            var userId = GetUserId();
            var result = await _notifService.MarkAsReadAsync(id, userId);

            if (!result)
                return NotFound("الإشعار غير موجود أو لا ينتمي إليك.");

            return Ok(new { message = "تم تحديث الإشعار بنجاح" });
        }

                // PUT /api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notifService.MarkAllAsReadAsync(GetUserId());
            return NoContent();
        }

        // DELETE /api/notifications/{id}
                [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("معرف الإشعار غير صالح.");

            var deleted = await _notifService.DeleteAsync(id, GetUserId());
            if (!deleted)
                return NotFound("الإشعار غير موجود أو لا ينتمي إليك.");

            return NoContent();
        }

        // DELETE /api/notifications/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> DeleteAll()
        {
            await _notifService.DeleteAllAsync(GetUserId());
            return NoContent();
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    }
}
