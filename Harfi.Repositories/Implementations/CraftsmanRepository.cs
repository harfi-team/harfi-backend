using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations
{
    public class CraftsmanRepository : GenericRepository<Craftsman>, ICraftsmanRepository
    {
        private readonly new AppDbContext _context;

        public CraftsmanRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Craftsman>> GetAllWithUserAsync()
        {
            return await _context.Craftsmen
                .Include(c => c.User)
                .Include(c => c.CityNavigation)
                .ToListAsync();
        }

        public IQueryable<Craftsman> GetAllWithUserQuery()
        {
            return _context.Craftsmen
                .Include(c => c.User)
                .Include(c => c.CityNavigation)
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task DeleteAsync(int craftsmanId)
        {
            var craftsman = await _context.Craftsmen.FindAsync(craftsmanId);
            if (craftsman is not null)
            {
                _context.Craftsmen.Remove(craftsman);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Craftsman>> GetFilteredCraftsmenAsync(CraftsmanFilterDto filter)
        {
            // 1. القائمة الأساسية للحرفيين المعتمدين والمتاحين
            var query = _context.Craftsmen
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.CityNavigation)
                .Where(c => c.IsApproved && c.IsAvailable)
                .AsQueryable();

            // 2. الفلترة بنوع الخدمة
            if (!string.IsNullOrWhiteSpace(filter.ServiceType))
            {
                var normalizedSearch = NormalizeSearchTerm(filter.ServiceType);

                // نجيب كل الخدمات من قاعدة البيانات
                // (ServiceTypes جدول صغير — غالباً أقل من 30 سجل)
                var allServices = await _context.ServiceTypes.AsNoTracking().ToListAsync();

                // نعمل الفلترة في الذاكرة (C#) لتجنب REPLACE التسلسلي في SQL
                var matchingIds = allServices
                    .Where(s => NormalizeSearchTerm(s.NameAr ?? "").Contains(normalizedSearch) ||
                               (s.NameEn ?? "").ToLower().Contains(normalizedSearch.ToLower()))
                    .Select(s => s.Id)
                    .ToHashSet();

                if (matchingIds.Count == 0)
                    return Enumerable.Empty<Craftsman>();

                query = query.Where(c => matchingIds.Contains(c.ServiceTypeId));
            }

            // 3. الفلترة بالمدينة (دعم اللغتين العربي والإنجليزي)
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var normalizedSearch = NormalizeSearchTerm(filter.City);

                // نجيب كل المدن (Cities جدول صغير — 18 مدينة)
                var allCities = await _context.Cities.AsNoTracking().ToListAsync();

                // نبحث عن أفضل مطابقة في الذاكرة
                var matchedIds = allCities
                    .Where(c => NormalizeSearchTerm(c.NameAr ?? "").Contains(normalizedSearch) ||
                               (c.NameEn ?? "").ToLower().Contains(normalizedSearch.ToLower()))
                    .Select(c => c.Id)
                    .ToHashSet();

                if (matchedIds.Count != 0)
                {
                    query = query.Where(c => matchedIds.Contains(c.CityId));
                }
                else
                {
                    // fallback — بحث مباشر في النص (إن كان البحث بالاسم ما لقى تطابق)
                    query = query.Where(c => c.CityNavigation != null &&
                        (c.CityNavigation.NameAr.Contains(normalizedSearch) ||
                         c.CityNavigation.NameEn.Contains(normalizedSearch)));
                }
            }

            // 4. الحد الأدنى للتقييم (يُطبق فقط إذا كان أكبر من صفر)
            // إذا كان 0، لا نقوم بالفلترة للسماح بظهور الحرفيين الجدد (null rating)
            if (filter.MinRating.HasValue && filter.MinRating > 0)
            {
                query = query.Where(c => c.Rating.HasValue && c.Rating >= filter.MinRating.Value);
            }

            // 5. الحد الأدنى لسنوات الخبرة (يُطبق فقط إذا كان أكبر من صفر)
            if (filter.MinExperience.HasValue && filter.MinExperience > 0)
            {
                query = query.Where(c => c.Experience >= filter.MinExperience.Value);
            }

            // 6. الترتيب التنازلي حسب التقييم (الحرفيين بدون تقييم يظهرون في النهاية)
            query = query.OrderByDescending(c => c.Rating.HasValue)
                         .ThenByDescending(c => c.Rating);

            return await query.ToListAsync();
        }

        private string NormalizeSearchTerm(string term)
        {
            if (string.IsNullOrEmpty(term)) return term;
            return term.Trim()
                       .Replace("+", " ")
                       .Replace("أ", "ا")
                       .Replace("إ", "ا")
                       .Replace("آ", "ا")
                       .Replace("ة", "ه")
                       .Replace("ى", "ي");
        }

        public async Task<Craftsman?> GetByUserIdAsync(int userId)
        {
            return await _context.Craftsmen
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> UpdateAsync(Craftsman craftsman)
        {
            _context.Craftsmen.Update(craftsman);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
