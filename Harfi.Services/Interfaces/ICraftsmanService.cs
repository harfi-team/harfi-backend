using Harfi.DTOs.Craftsman;
using Microsoft.AspNetCore.Http;

namespace Harfi.Services.Interfaces
{
    public interface ICraftsmanService
    {
        Task<bool> RegisterCraftsmanAsync(CreateCraftsmanDto createCraftsmanDto);
        Task<CraftsmanDto?> GetCraftsmanProfileAsync(int id);
        Task<bool> UpdateCraftsmanAsync(int id, UpdateCraftsmanDto dto);
        Task<IEnumerable<CraftsmanDto>> GetFilteredCraftsmenAsync(CraftsmanFilterDto filter);
        Task<string?> UploadProfileImageAsync(int craftsmanId, IFormFile file);
        Task<IEnumerable<ServiceLookupDto>> GetActiveServicesAsync();
        Task<IEnumerable<CityLookupDto>> GetActiveCitiesAsync();
        Task<string?> UploadNationalIdAsync(int craftsmanId, IFormFile file);
    }
}
