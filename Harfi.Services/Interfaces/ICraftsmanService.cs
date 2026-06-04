using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.Craftsman;

namespace Harfi.Services.Interfaces
{
    public interface ICraftsmanService
    {
        Task<bool> RegisterCraftsmanAsync(CreateCraftsmanDto createCraftsmanDto);
        Task<CraftsmanDto?> GetCraftsmanProfileAsync(int id);

        Task<bool> UpdateCraftsmanAsync(int id, UpdateCraftsmanDto dto);
        Task<IEnumerable<CraftsmanDto>> GetFilteredCraftsmenAsync(CraftsmanFilterDto filter);
    }
}
