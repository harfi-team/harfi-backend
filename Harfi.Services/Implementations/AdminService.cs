using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class AdminService:IAdminService
    {
        private readonly ICraftsmanRepository _craftsmanRepository;

        // حقن الـ CraftsmanRepository المخصص لأنه يحتوي على الإمكانيات المتقدمة للتعامل مع جدول الحرفيين
        public AdminService(ICraftsmanRepository craftsmanRepository)
        {
            _craftsmanRepository = craftsmanRepository;
        }

        // 1. جلب كل الحرفيين المعلقين (الذين ينتظرون موافقة الأدمن IsApproved == false)
        public async Task<IEnumerable<CraftsmanDto>> GetPendingCraftsmenAsync()
        {
            // جلب كل البيانات من الـ Repository
            var allCraftsmen = await _craftsmanRepository.GetAllAsync();

            return allCraftsmen
                .Where(c => !c.IsApproved)
                .Select(c => new CraftsmanDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User?.Name, 
                    Email = c.User?.Email,
                    Phone = c.User?.Phone,
                    ServiceType = c.ServiceType,
                    City = c.City,
                    Experience = c.Experience,
                    NationalIdUrl = c.NationalIdUrl, // رابط البطاقة الشخصية لكي يراجعها الأدمن
                    CreatedAt = c.CreatedAt
                });
        }

        // 2. الموافقة على الحرفي وتفعيل حسابه ليظهر في نتائج البحث
        public async Task<bool> ApproveCraftsmanAsync(int craftsmanId)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(craftsmanId);
            if (craftsman == null) return false;

            craftsman.IsApproved = true; // تحويل الحالة إلى مقبول

          await  _craftsmanRepository.UpdateAsync(craftsman); 
            return true;
        }

        // 3. رفض الحرفي (يمكن مسحه نهائياً أو تغيير حالته بحسب رغبة التيم)
        // هنا قمنا بعمل مسح (Delete) لأن طلب التسجيل رُفض تماماً
        public async Task<bool> RejectCraftsmanAsync(int craftsmanId)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(craftsmanId);
            if (craftsman == null) return false;

           await  _craftsmanRepository.DeleteAsync(  craftsmanId); 
            return true;
        }
    }
}
