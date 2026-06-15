using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        /// <summary>Includes soft-deleted rows, bypassing the global query filter. Needed for Restore.</summary>
        Task<Category?> GetByIdIncludingDeletedAsync(int id);

        Task<IEnumerable<Category>> GetAllIncludingDeletedAsync();

        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
