using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces;

public interface ICraftsmanRepository
{
    Task<List<Craftsman>> GetAllAsync();
    Task<Craftsman?> GetByIdAsync(int id);
    Task<List<Craftsman>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<Craftsman>> FilterAsync(string? serviceType, string? city, double? minRating);
}