using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class CraftsmanService:ICraftsmanService
    {
        private readonly ICraftsmanRepository _craftsmanRepository;

        public CraftsmanService(ICraftsmanRepository craftsmanRepository)
        {
            _craftsmanRepository = craftsmanRepository;
        }

        // 1. تسجيل حرفي جديد (بيكون معلق IsApproved = false في البداية)
        public async Task<bool> RegisterCraftsmanAsync(CreateCraftsmanDto createCraftsmanDto)
        {
            var craftsman = new Craftsman
            {
                UserId = createCraftsmanDto.UserId,
                ServiceType = createCraftsmanDto.ServiceType,
                City = createCraftsmanDto.City,
                Neighborhood = createCraftsmanDto.Neighborhood,
                PriceRangeMin = createCraftsmanDto.PriceRangeMin,
                PriceRangeMax = createCraftsmanDto.PriceRangeMax,
                Experience = (int)createCraftsmanDto.Experience,
                Bio = createCraftsmanDto.Bio,
                NationalIdUrl = createCraftsmanDto.NationalIdUrl,
                IsApproved = false, // افتراضياً غير مقبول لحين مراجعة الأدمن
                IsAvailable = true,
                Rating = 0, // يبدأ بتقييم صفر
                CreatedAt = DateTime.UtcNow
            };

            await _craftsmanRepository.AddAsync(craftsman);
            return true;
        }

        // 2. جلب بروفايل حرفي معين بكامل بياناته المهنية والشخصية
        public async Task<CraftsmanDto> GetCraftsmanProfileAsync(int id)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(id); 
            if (craftsman == null) return null;

            return new CraftsmanDto
            {
                Id = craftsman.Id,
                UserId = craftsman.UserId,
                FullName = craftsman.User?.Name, // جلب الاسم من جدول اليوزر بفضل الـ Include
                Email = craftsman.User?.Email,
                Phone = craftsman.User?.Phone,
                ProfileImageUrl = craftsman.User?.ProfileImageUrl,
                City = craftsman.City,
                Neighborhood = craftsman.Neighborhood,
                PriceRangeMin = craftsman.PriceRangeMin,
                PriceRangeMax = craftsman.PriceRangeMax,
                Experience = craftsman.Experience,
                IsApproved = craftsman.IsApproved,
                IsAvailable = craftsman.IsAvailable,
                Rating = craftsman.Rating,
                Bio = craftsman.Bio,
                NationalIdUrl = craftsman.NationalIdUrl,
                CreatedAt = craftsman.CreatedAt
            };
        }

        // 3. البحث والفلترة الديناميكية المترتبة تنازلياً حسب التقييم
        public async Task<IEnumerable<CraftsmanDto>> GetFilteredCraftsmenAsync(CraftsmanFilterDto filter)
        {
            var craftsmen = await _craftsmanRepository.GetFilteredCraftsmenAsync(filter);

            // تحويل الليستة لـ DTOs للعرض في الـ Frontend
            return craftsmen.Select(c => new CraftsmanDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User?.Name,
                Email = c.User?.Email,
                Phone = c.User?.Phone,
                ProfileImageUrl = c.User?.ProfileImageUrl,
                ServiceType = c.ServiceType,
                City = c.City,
                Neighborhood = c.Neighborhood,
                PriceRangeMin = c.PriceRangeMin,
                PriceRangeMax = c.PriceRangeMax,
                Experience = c.Experience,
                IsApproved = c.IsApproved,
                IsAvailable = c.IsAvailable,
                Rating = c.Rating,
                Bio = c.Bio,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<bool> UpdateCraftsmanAsync(int id, UpdateCraftsmanDto dto)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(id); 
            if (craftsman == null) return false;

            // تحديث البيانات=
            craftsman.City = dto.City;
            craftsman.Neighborhood = dto.Neighborhood;
            craftsman.PriceRangeMin = dto.PriceRangeMin;
            craftsman.PriceRangeMax = dto.PriceRangeMax;
            craftsman.Experience = dto.Experience;
            craftsman.Bio = dto.Bio;
            craftsman.UpdatedAt = DateTime.UtcNow; 

            return await _craftsmanRepository.UpdateAsync(craftsman);
        }

    }
}
