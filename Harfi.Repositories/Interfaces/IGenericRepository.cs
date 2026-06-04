using System.Linq.Expressions;

namespace Harfi.Repositories.Interfaces;

/// <summary>
/// Generic repository interface — covers all CRUD operations.
/// Every entity-specific repository inherits from this.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    // ── READ ──────────────────────────────────────────────────
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task LoadReferenceAsync<TProperty>(T entity,Expression<Func<T, TProperty?>> navigationProperty)where TProperty : class;

    // ── WRITE ─────────────────────────────────────────────────
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // ── SAVE ──────────────────────────────────────────────────
    Task<int> SaveChangesAsync();
}
