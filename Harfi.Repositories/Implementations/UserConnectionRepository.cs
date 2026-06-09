using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations;

public class UserConnectionRepository
    : GenericRepository<UserConnection>, IUserConnectionRepository
{
    public UserConnectionRepository(AppDbContext context) 
        : base(context) { }

    public async Task<UserConnection?> GetByConnectionIdAsync(
        string connectionId)
        => await _dbSet.FirstOrDefaultAsync(
            c => c.ConnectionId == connectionId);
}
