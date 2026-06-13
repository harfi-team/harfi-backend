using Harfi.DTOs.Craftsman;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Harfi.Services.Implementations
{
    public class CraftsmanService : ICraftsmanService
    {
        private readonly ICraftsmanRepository _craftsmanRepository;
        private readonly UserManager<User> _userManager;
        private readonly IImageservice _imageService;

        public CraftsmanService(
            ICraftsmanRepository craftsmanRepository,
            UserManager<User> userManager,
            IImageservice imageService)
        {
            _craftsmanRepository = craftsmanRepository;
            _userManager = userManager;
            _imageService = imageService;
        }

        // 1. تسجيل حرفي جديد (بيكون معلق IsApproved = false في البداية)
        public async Task<bool> RegisterCraftsmanAsync(CreateCraftsmanDto createCraftsmanDto)
        {
            // 1. التحقق من وجود المستخدم
            var user = await _userManager.FindByIdAsync(createCraftsmanDto.UserId.ToString());
            if (user is null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            // 2. التحقق من أن دور المستخدم هو "craftsman"
            if (user.Role != "craftsman")
                throw new InvalidOperationException(
                    "هذا المستخدم ليس لديه صلاحية التسجيل كحرفي.");

            // 3. التأكد من عدم وجود سجل حرفي مسبق لنفس المستخدم (علاقة 1:1)
            var exists = await _craftsmanRepository.ExistsAsync(c => c.UserId == createCraftsmanDto.UserId);
            if (exists)
                throw new InvalidOperationException(
                    "هذا المستخدم مسجل كحرفي مسبقاً.");

            var craftsman = new Craftsman
            {
                UserId = createCraftsmanDto.UserId,
                ServiceType = createCraftsmanDto.ServiceType,
                City = createCraftsmanDto.City,
                Neighborhood = createCraftsmanDto.Neighborhood,
                PriceRangeMin = createCraftsmanDto.PriceRangeMin,
                PriceRangeMax = createCraftsmanDto.PriceRangeMax,
                Experience = createCraftsmanDto.Experience ?? 0,
                Bio = createCraftsmanDto.Bio,
                NationalIdUrl = createCraftsmanDto.NationalIdUrl,
                IsApproved = false,
                IsAvailable = true,
                Rating = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _craftsmanRepository.AddAsync(craftsman);
            await _craftsmanRepository.SaveChangesAsync();
            return true;
        }

        // 2. جلب بروفايل حرفي معين بكامل بياناته المهنية والشخصية
        public async Task<CraftsmanDto?> GetCraftsmanProfileAsync(int id)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(id);
            if (craftsman == null) return null;

            // تحميل بيانات المستخدم المرتبط لتجنب null reference
            await _craftsmanRepository.LoadReferenceAsync(craftsman, c => c.User);

            return new CraftsmanDto
            {
                Id = craftsman.Id,
                UserId = craftsman.UserId,
                FullName = craftsman.User?.Name ?? string.Empty,
                Email = craftsman.User?.Email ?? string.Empty,
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
                FullName = c.User?.Name ?? string.Empty,
                Email = c.User?.Email ?? string.Empty,
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

        public async Task<string?> UploadProfileImageAsync(int craftsmanId, IFormFile file)
        {
            var craftsman = await _craftsmanRepository.GetByIdAsync(craftsmanId);
            if (craftsman == null) return null;

            var user = await _userManager.FindByIdAsync(craftsman.UserId.ToString());
            if (user == null) return null;

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                _imageService.DeleteImage(user.ProfileImageUrl, "profiles");

            var imageUrl = await _imageService.SaveImageAsync(file, "profiles");

            user.ProfileImageUrl = imageUrl;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? imageUrl : null;
        }
        // غيري الـ Return Type في الـ Interface والـ Service للـ DTOs الجديدة
        public async Task<IEnumerable<ServiceLookupDto>> GetActiveServicesAsync()
        {
            var activeServices = await _craftsmanRepository.GetActiveServicesAsync();

            // تحويل كل اسم خدمة عربي للـ DTO الكامل باستخدام الدالة المساعدة
            return activeServices.Select(serviceAr => MapServiceDetails(serviceAr));
        }

        public async Task<IEnumerable<CityLookupDto>> GetActiveCitiesAsync()
        {
            var activeCities = await _craftsmanRepository.GetActiveCitiesAsync();

            // تحويل كل اسم مدينة عربي للـ DTO الكامل
            return activeCities.Select(cityAr => MapCityDetails(cityAr));
        }

        // ---------------- الدوال المساعدة (Translators) ----------------

        private ServiceLookupDto MapServiceDetails(string serviceAr)
        {
            // استخدام Switch Expression (طريقة حديثة ونظيفة جداً في C#)
            return serviceAr.Trim() switch
            {
                "سباكة" => new ServiceLookupDto { NameAr = "سباكة", NameEn = "Plumbing", Icon = "plumbing" },
                "كهرباء" => new ServiceLookupDto { NameAr = "كهرباء", NameEn = "Electrical", Icon = "electric_bolt" },
                "دهانات" => new ServiceLookupDto { NameAr = "دهانات", NameEn = "Painting", Icon = "format_paint" },
                "نجارة" => new ServiceLookupDto { NameAr = "نجارة", NameEn = "Carpentry", Icon = "carpenter" },
                "تكييف وتبريد" => new ServiceLookupDto { NameAr = "تكييف وتبريد", NameEn = "HVAC & Cooling", Icon = "ac_unit" },
                "تبريد وتكييف" => new ServiceLookupDto { NameAr = "تبريد وتكييف", NameEn = "HVAC / AC", Icon = "ac_unit" }, // للحفاظ على التوافق القديم
                "ألمنيوم" => new ServiceLookupDto { NameAr = "ألمنيوم", NameEn = "Aluminum", Icon = "window" },
                "أعمال ألمنيوم" => new ServiceLookupDto { NameAr = "أعمال ألمنيوم", NameEn = "Aluminum Works", Icon = "window" }, // للحفاظ على التوافق القديم
                "أمن وكاميرات" => new ServiceLookupDto { NameAr = "أمن وكاميرات", NameEn = "Security & Cameras", Icon = "videocam" },
                "تبليط وسيراميك" => new ServiceLookupDto { NameAr = "تبليط وسيراميك", NameEn = "Tiling & Ceramics", Icon = "grid_on" },
                "جبس وأسقف" => new ServiceLookupDto { NameAr = "جبس وأسقف", NameEn = "Gypsum & Ceilings", Icon = "texture" },
                "حدادة" => new ServiceLookupDto { NameAr = "حدادة", NameEn = "Blacksmith", Icon = "shield_with_heart" },
                "زجاج ومرايا" => new ServiceLookupDto { NameAr = "زجاج ومرايا", NameEn = "Glass & Mirrors", Icon = "filter_frames" },
                "مكافحة حشرات" => new ServiceLookupDto { NameAr = "مكافحة حشرات", NameEn = "Pest Control", Icon = "pest_control" },

                // إضافات عامة أخرى (من الكود القديم الخاص بك)
                "نظافة" => new ServiceLookupDto { NameAr = "نظافة", NameEn = "Cleaning", Icon = "cleaning_services" },
                "بناء" => new ServiceLookupDto { NameAr = "بناء", NameEn = "Construction", Icon = "construction" },
                "صيانة عامة" => new ServiceLookupDto { NameAr = "صيانة عامة", NameEn = "General Maintenance", Icon = "build" },
                "نقل أثاث" => new ServiceLookupDto { NameAr = "نقل أثاث", NameEn = "Furniture Moving", Icon = "local_shipping" },
                "جبس" => new ServiceLookupDto { NameAr = "جبس", NameEn = "Gypsum", Icon = "texture" },

                // الـ Fallback
                _ => new ServiceLookupDto { NameAr = serviceAr, NameEn = serviceAr, Icon = "build_circle" }
            };
        }

        private CityLookupDto MapCityDetails(string cityAr)
        {
            return cityAr.Trim() switch
            {
                "القاهرة" => new CityLookupDto { NameAr = "القاهرة", NameEn = "Cairo" },
                "الإسكندرية" => new CityLookupDto { NameAr = "الإسكندرية", NameEn = "Alexandria" },
                "الجيزة" => new CityLookupDto { NameAr = "الجيزة", NameEn = "Giza" },
                "المنصورة" => new CityLookupDto { NameAr = "المنصورة", NameEn = "Mansoura" },
                "أسيوط" => new CityLookupDto { NameAr = "أسيوط", NameEn = "Assiut" },
                "الإسماعيلية" => new CityLookupDto { NameAr = "الإسماعيلية", NameEn = "Ismailia" },
                "الأقصر" => new CityLookupDto { NameAr = "الأقصر", NameEn = "Luxor" },
                "الفيوم" => new CityLookupDto { NameAr = "الفيوم", NameEn = "Fayoum" },
                "المنيا" => new CityLookupDto { NameAr = "المنيا", NameEn = "Minya" },
                "بني سويف" => new CityLookupDto { NameAr = "بني سويف", NameEn = "Beni Suef" },
                "بورسعيد" => new CityLookupDto { NameAr = "بورسعيد", NameEn = "Port Said" },
                "دمنهور" => new CityLookupDto { NameAr = "دمنهور", NameEn = "Damanhour" },
                "سوهاج" => new CityLookupDto { NameAr = "سوهاج", NameEn = "Sohag" },
                // Fallback
                _ => new CityLookupDto { NameAr = cityAr, NameEn = cityAr }
            };
        }

    }
}
