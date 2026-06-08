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
                .ToListAsync();
        }

        public IQueryable<Craftsman> GetAllWithUserQuery()
        {
            return _context.Craftsmen
                .Include(c => c.User)
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
            // 1. جلب الحرفيين المعتمدين من الداتابيز

            var query = _context.Craftsmen
                                .Include(c => c.User)
                                .Where(c => c.IsApproved)
                                .AsQueryable();

            // 2. الفلترة بنوع الخدمة (تعديل الـ == إلى .Contains لدعم البحث العربي الجزئي)
            if (!string.IsNullOrEmpty(filter.ServiceType))
            {
                query = query.Where(c => c.ServiceType.Contains(filter.ServiceType));
            }

            // 3. الفلترة بالمدينة (تعديل الـ == إلى .Contains لدعم البحث العربي الجزئي)
            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(c => c.City.Contains(filter.City));
            }

            // 4. الحد الأدنى للتقييم
            if (filter.MinRating.HasValue)
            {
                query = query.Where(c => c.Rating >= filter.MinRating.Value);
            }

            // 5. الحد الأدنى لسنوات الخبرة
            if (filter.MinExperience.HasValue)
            {
                query = query.Where(c => c.Experience >= filter.MinExperience.Value);
            }

            // 6. الترتيب من الأعلى تقييماً للأقل
            query = query.OrderByDescending(c => c.Rating);

            // 7. تنفيذ الكود وإرجاع النتائج
            return await query.ToListAsync();
        }

        public async Task<bool> UpdateAsync(Craftsman craftsman)
        {
            _context.Craftsmen.Update(craftsman);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
