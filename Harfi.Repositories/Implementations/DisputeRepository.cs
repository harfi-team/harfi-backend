using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations;

public class DisputeRepository : IDisputeRepository
{
    private readonly AppDbContext _context;

    public DisputeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dispute?> GetByIdAsync(int id)
        => await _context.Disputes
            .Include(d => d.Job).ThenInclude(j => j.Customer)
            .Include(d => d.Job).ThenInclude(j => j.Craftsman!).ThenInclude(c => c.User)
            .Include(d => d.RaisedByUser)
            .Include(d => d.ResolvedByAdmin)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Dispute?> GetActiveDisputeForJobAsync(int jobId)
        => await _context.Disputes
            .Where(d => d.JobId == jobId && DisputeStatusConstants.Active.Contains(d.Status))
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<bool> HasActiveDisputeAsync(int jobId)
        => await _context.Disputes
            .AnyAsync(d => d.JobId == jobId && DisputeStatusConstants.Active.Contains(d.Status));

    public async Task<IEnumerable<Dispute>> GetByUserIdAsync(int userId)
        => await _context.Disputes
            .Include(d => d.Job)
            .Where(d => d.RaisedByUserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public async Task<Dispute> CreateAsync(Dispute dispute)
    {
        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();
        return dispute;
    }

    public async Task<Dispute> UpdateAsync(Dispute dispute)
    {
        _context.Disputes.Update(dispute);
        await _context.SaveChangesAsync();
        return dispute;
    }

    public async Task<int> CountActiveAsync()
        => await _context.Disputes
            .CountAsync(d => DisputeStatusConstants.Active.Contains(d.Status));

    public IQueryable<Dispute> GetQueryable()
        => _context.Disputes.AsNoTracking().AsQueryable();
}
