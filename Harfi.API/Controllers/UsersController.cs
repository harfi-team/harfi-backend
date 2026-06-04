using Harfi.DTOs.User;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var profile = await _userService.GetUserProfileAsync(id);

            if (profile == null)
                return NotFound(new { message = "عذراً، هذا المستخدم غير موجود." });

            return Ok(profile);
        }

        // 2. تحديث بروفايل المستخدم
        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserDto dto)
        {
            // أضفنا فحص الـ ModelState هنا للتأكد من سلامة المدخلات
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserProfileAsync(id, dto);

            if (!result)
                return BadRequest(new { message = "فشل في تحديث البيانات، يرجى التحقق من المدخلات والمحاولة مرة أخرى." });

            return Ok(new { message = "تم تحديث بيانات الملف الشخصي بنجاح." });
        }
    }
}
