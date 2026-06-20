using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Craftsman?> GetCraftsmanByUserIdAsync(int userId)
    => await _context.Craftsmen.FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<bool> CraftsmanBelongsToUserAsync(int craftsmanId, int userId)
        => await _context.Craftsmen
            .AnyAsync(c => c.Id == craftsmanId && c.UserId == userId);

    public async Task<Job?> GetByIdAsync(int id)
        => await _context.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Craftsman).ThenInclude(c => c!.User)
            .Include(j => j.Conversation)
            .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<IEnumerable<Job>> GetByCustomerIdAsync(int customerId)
        => await _context.Jobs
            .Include(j => j.Conversation)
            .Where(j => j.CustomerId == customerId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Job>> GetByCraftsmanIdAsync(int craftsmanId)
        => await _context.Jobs
            .Include(j => j.Conversation)
            .Where(j => j.CraftsmanId == craftsmanId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

    public async Task<Job> CreateAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<Job> UpdateAsync(Job job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

}