using Harfi.DTOs.Craftsman;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CraftsmenController : ControllerBase
    {
        private readonly ICraftsmanService _craftsmanService;
        private readonly IImageservice _imageService;

        public CraftsmenController(ICraftsmanService craftsmanService, IImageservice imageService)
        {
            _craftsmanService = craftsmanService;
            _imageService = imageService;
        }

        // 1. تقديم طلب تسجيل الحرفي
        // 1. تقديم طلب تسجيل الحرفي
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] CreateCraftsmanDto dto) // التعديل هنا: FromForm
        {
            try
            {
                var result = await _craftsmanService.RegisterCraftsmanAsync(dto);

                if (!result)
                    return BadRequest(new { message = "فشل في تقديم طلب التسجيل، يرجى المحاولة مرة أخرى." });

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "تم تقديم طلبك بنجاح وهو قيد المراجعة حالياً."
                });
            }
            catch (ArgumentException ex)
            {
                // لمسك أخطاء رفع الصورة (زي الحجم أو الصيغة)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // لمسك أخطاء الداتابيز (زي المستخدم موجود مسبقاً)
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. جلب الملف الشخصي للحرفي بواسطة الـ ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(int id)
        {
            var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);

            if (profile == null)
                return NotFound(new { message = "عذراً، هذا الحرفي غير موجود حالياً." });

            return Ok(profile);
        }

        // 3. البحث والفلترة المتقدمة (يدعم العربي والإنجليزي)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] CraftsmanFilterDto filter)
        {
            var results = await _craftsmanService.GetFilteredCraftsmenAsync(filter);

            // إذا لم يتم العثور على أي نتائج تطابق البحث
            if (results == null || !results.Any())
            {
                return NotFound(new { message = "لم يتم العثور على أي حرفيين يطابقون محددات البحث الحالية." });
            }

            return Ok(results);
        }

        // 4. تحديث بيانات الحرفي (الاسم والصورة وباقي التفاصيل)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateCraftsmanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requestingRole = User.FindFirstValue(ClaimTypes.Role);
            if (requestingRole != "admin")
            {
                var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);
                if (profile == null)
                    return NotFound(new { message = "لم يتم العثور على حساب الحرفي المطلوب لتحديثه." });
                if (profile.UserId != requestingUserId)
                    return Forbid();
            }

            var result = await _craftsmanService.UpdateCraftsmanAsync(id, dto);

            if (!result)
                return NotFound(new { message = "لم يتم العثور على حساب الحرفي المطلوب لتحديثه." });

            return Ok(new { message = "تم تحديث بيانات الملف الشخصي بنجاح." });
        }

        // 5. رفع صورة البروفايل بنسنخدمها من اليوزر مش من هنا 
        //[HttpPost("{id}/upload-image")]
        //public async Task<IActionResult> UploadProfileImage(int id, [FromForm] IFormFile file)
        //{
        //    var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //    var requestingRole = User.FindFirstValue(ClaimTypes.Role);
        //    if (requestingRole != "admin")
        //    {
        //        var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);
        //        if (profile == null)
        //            return NotFound(new { message = "عذراً، هذا الحرفي غير موجود حالياً." });
        //        if (profile.UserId != requestingUserId)
        //            return Forbid();
        //    }

        //    if (file == null || file.Length == 0)
        //        return BadRequest(new { message = "الرجاء اختيار صورة للرفع." });

        //    var url = await _craftsmanService.UploadProfileImageAsync(id, file);

        //    if (url == null)
        //        return NotFound(new { message = "عذراً، هذا الحرفي غير موجود حالياً." });

        //    return Ok(new { url });
        //}






        [HttpPost("{id}/upload-national-id")]
        public async Task<IActionResult> UploadNationalId([FromRoute] int id, [FromForm] UploadNationalIdDto dto)
        {
            // باقي الكود بتاعك زي ما هو بالظبط بدون أي تغيير...
            var requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requestingRole = User.FindFirstValue(ClaimTypes.Role);

            if (requestingRole != "admin")
            {
                var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);
                if (profile == null)
                    return NotFound(new { message = "عذراً، هذا الحرفي غير موجود حالياً." });

                if (profile.UserId != requestingUserId)
                    return Forbid();
            }

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "الرجاء اختيار ملف بطاقة الهوية للرفع." });

            try
            {
                var url = await _craftsmanService.UploadNationalIdAsync(id, dto.File);

                if (url == null)
                    return NotFound(new { message = "عذراً، لم يتم العثور على بيانات الحرفي." });

                return Ok(new { url });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "حدث خطأ غير متوقع أثناء رفع الملف." });
            }
        }



        [HttpGet("active-services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveServices()
        {
            var services = await _craftsmanService.GetActiveServicesAsync();

            if (!services.Any())
            {
                return NotFound(new { message = "لم يتم العثور على أي خدمات متاحة حالياً." });
            }

            return Ok(services); // هيرجع JSON فيه NameAr, NameEn, Icon
        }

        [HttpGet("active-cities")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCities()
        {
            var cities = await _craftsmanService.GetActiveCitiesAsync();

            if (!cities.Any())
            {
                return NotFound(new { message = "لم يتم العثور على أي مدن يتواجد بها حرفيون حالياً." });
            }

            return Ok(cities); // هيرجع JSON فيه NameAr, NameEn
        }
    }
}