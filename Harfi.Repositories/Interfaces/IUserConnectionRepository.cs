using Harfi.Models.Entities;

namespace Harfi.Repositories.Interfaces;

public interface IUserConnectionRepository
    : IGenericRepository<UserConnection>
{
    Task<UserConnection?> GetByConnectionIdAsync(string connectionId);
}