using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;
using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces
{
    public interface ICraftsmanRepository:IGenericRepository<Craftsman>
    {
        Task DeleteAsync(int craftsmanId);

        // الدالة المخصصة للبحث المتقدم والفلترة الديناميكية
        Task<IEnumerable<Craftsman>> GetFilteredCraftsmenAsync(CraftsmanFilterDto filter);
        Task<bool> UpdateAsync(Craftsman craftsman);
    }
}
