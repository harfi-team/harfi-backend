using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations;

public class CraftsmanRepository : ICraftsmanRepository
{
    private readonly AppDbContext _db;
    public CraftsmanRepository(AppDbContext db) => _db = db;

    public async Task<List<Craftsman>> GetAllAsync()
        => await _db.Craftsmen
               .Include(c => c.User)
               .Where(c => c.IsApproved)
               .AsNoTracking()
               .ToListAsync();

    public async Task<Craftsman?> GetByIdAsync(int id)
        => await _db.Craftsmen
               .Include(c => c.User)
               .AsNoTracking()
               .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Craftsman>> GetByIdsAsync(IEnumerable<int> ids)
        => await _db.Craftsmen
               .Include(c => c.User)
               .Where(c => ids.Contains(c.Id))
               .AsNoTracking()
               .ToListAsync();

    public async Task<List<Craftsman>> FilterAsync(
        string? serviceType, string? city, double? minRating)
    {
        var q = _db.Craftsmen
                   .Include(c => c.User)
                   .Where(c => c.IsApproved)
                   .AsNoTracking()
                   .AsQueryable();

        if (!string.IsNullOrWhiteSpace(serviceType))
            q = q.Where(c => c.ServiceType == serviceType);

        if (!string.IsNullOrWhiteSpace(city))
            q = q.Where(c => c.City == city);

        if (minRating.HasValue)
            q = q.Where(c => (double)c.Rating >= minRating.Value);

        return await q.OrderByDescending(c => c.Rating).ToListAsync();
    }
}
