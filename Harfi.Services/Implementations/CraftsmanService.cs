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
                ServiceTypeId = createCraftsmanDto.ServiceTypeId,
                CityId = createCraftsmanDto.CityId,
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

            // تحميل بيانات المستخدم والخدمة والمدينة المرتبطة
            await _craftsmanRepository.LoadReferenceAsync(craftsman, c => c.User);
            await _craftsmanRepository.LoadReferenceAsync(craftsman, c => c.Service);
            await _craftsmanRepository.LoadReferenceAsync(craftsman, c => c.CityNavigation);

            return new CraftsmanDto
            {
                Id = craftsman.Id,
                UserId = craftsman.UserId,
                FullName = craftsman.User?.Name ?? string.Empty,
                Email = craftsman.User?.Email ?? string.Empty,
                Phone = craftsman.User?.Phone,
                ProfileImageUrl = craftsman.User?.ProfileImageUrl,
                ServiceTypeId = craftsman.ServiceTypeId,
                ServiceNameAr = craftsman.Service?.NameAr,
                ServiceNameEn = craftsman.Service?.NameEn,
                ServiceName = craftsman.Service?.NameAr, // Default to Arabic
                CityId = craftsman.CityId,
                CityNameAr = craftsman.CityNavigation?.NameAr,
                CityNameEn = craftsman.CityNavigation?.NameEn,
                Governorate = craftsman.CityNavigation?.Governorate,
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
                ServiceTypeId = c.ServiceTypeId,
                ServiceNameAr = c.Service?.NameAr,
                ServiceNameEn = c.Service?.NameEn,
                ServiceName = c.Service?.NameAr, // Default to Arabic
                CityId = c.CityId,
                CityNameAr = c.CityNavigation?.NameAr,
                CityNameEn = c.CityNavigation?.NameEn,
                Governorate = c.CityNavigation?.Governorate,
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
            craftsman.ServiceTypeId = dto.ServiceTypeId;
            craftsman.CityId = dto.CityId;
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
    }
}
