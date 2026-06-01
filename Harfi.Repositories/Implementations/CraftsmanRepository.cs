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

            var query = _context.Craftsmen
                .Include(c => c.User)
                .Where(c => c.IsApproved)
                .AsQueryable();


            // الفلترة بنوع الخدمة (مثلاً: سباكة)
            if (!string.IsNullOrEmpty(filter.ServiceType))
            {
                query = query.Where(c => c.ServiceType == filter.ServiceType);
            }

            // الفلترة بالمدينة
            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(c => c.City.ToLower() == filter.City.ToLower());
            }

            // الفلترة بالحد الأدنى للتقييم
            if (filter.MinRating.HasValue)
            {
                query = query.Where(c => c.Rating >= filter.MinRating.Value);
            }

            // الفلترة بالحد الأدنى لسنوات الخبرة
            if (filter.MinExperience.HasValue)
            {
                query = query.Where(c => c.Experience >= filter.MinExperience.Value);
            }

            // 3. الترتيب الحرفي: حسب الأعلى تقييماً (Sorted by rating) زي ما مطلوب في التاسك بالظبط
            query = query.OrderByDescending(c => c.Rating);

            // التنفيذ النهائي وجلب البيانات من قاعدة البيانات
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
