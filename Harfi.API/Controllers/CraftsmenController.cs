using Harfi.DTOs.Craftsman;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CraftsmenController : ControllerBase
    {
        private readonly ICraftsmanService _craftsmanService;

        public CraftsmenController(ICraftsmanService craftsmanService)
        {
            _craftsmanService = craftsmanService;
        }

        // 1. تقديم طلب تسجيل الحرفي
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateCraftsmanDto dto)
        {
            var result = await _craftsmanService.RegisterCraftsmanAsync(dto);

            if (!result)
                return BadRequest(new { message = "فشل في تقديم طلب التسجيل، يرجى المحاولة مرة أخرى." });

            return Ok(new { message = "تم تقديم طلبك بنجاح وهو قيد المراجعة حالياً." });
        }

        // 2. جلب الملف الشخصي للحرفي بواسطة الـ ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);

            if (profile == null)
                return NotFound(new { message = "عذراً، هذا الحرفي غير موجود حالياً." });

            return Ok(profile);
        }

        // 3. البحث والفلترة المتقدمة (يدعم العربي والإنجليزي)
        [HttpGet("search")]
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

            var result = await _craftsmanService.UpdateCraftsmanAsync(id, dto);

            if (!result)
                return NotFound(new { message = "لم يتم العثور على حساب الحرفي المطلوب لتحديثه." });

            return Ok(new { message = "تم تحديث بيانات الملف الشخصي بنجاح." });
        }
    }
}