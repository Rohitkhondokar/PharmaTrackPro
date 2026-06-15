using System.Linq.Expressions;

namespace PharmaTrackPro.Interfaces
{
    /// <summary>
    /// Generic data-access contract reused by every module's repository.
    /// Keeps CRUD boilerplate out of individual repositories (DRY).
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> SaveChangesAsync();
    }
}
