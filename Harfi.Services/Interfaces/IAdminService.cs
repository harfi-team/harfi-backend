using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;

namespace Harfi.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<CraftsmanDto>> GetPendingCraftsmenAsync();
        Task<bool> ApproveCraftsmanAsync(int craftsmanId);
        Task<bool> RejectCraftsmanAsync(int craftsmanId);
    }
}

