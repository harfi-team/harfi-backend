using Harfi.DTOs.User;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. جلب بروفايل المستخدم
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requestingRole = User.FindFirstValue(ClaimTypes.Role);
            if (requestingUserId != id && requestingRole != "admin")
                return Forbid();

            var profile = await _userService.GetUserProfileAsync(id);

            if (profile == null)
                return NotFound(new { message = "عذراً، هذا المستخدم غير موجود." });

            return Ok(profile);
        }

        // 2. تحديث بروفايل المستخدم
        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserDto dto)
        {
            var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requestingRole = User.FindFirstValue(ClaimTypes.Role);
            if (requestingUserId != id && requestingRole != "admin")
                return Forbid();

            // أضفنا فحص الـ ModelState هنا للتأكد من سلامة المدخلات
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserProfileAsync(id, dto);

            if (!result)
                return BadRequest(new { message = "فشل في تحديث البيانات، يرجى التحقق من المدخلات والمحاولة مرة أخرى." });

            return Ok(new { message = "تم تحديث بيانات الملف الشخصي بنجاح." });
        }

        // 3. رفع صورة البروفايل
        [HttpPost("profile/{id}/upload-image")]
        public async Task<IActionResult> UploadProfileImage(int id, [FromForm] IFormFile file)
        {
            var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requestingRole = User.FindFirstValue(ClaimTypes.Role);
            if (requestingUserId != id && requestingRole != "admin")
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "الرجاء اختيار صورة للرفع." });

            var url = await _userService.UploadProfileImageAsync(id, file);

            if (url == null)
                return NotFound(new { message = "عذراً، هذا المستخدم غير موجود." });

            return Ok(new { url });
        }
    }
}
