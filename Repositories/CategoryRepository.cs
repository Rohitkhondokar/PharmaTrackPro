using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByIdIncludingDeletedAsync(int id) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Category>> GetAllIncludingDeletedAsync() =>
            await DbSet.IgnoreQueryFilters().OrderBy(c => c.Name).ToListAsync();

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            // Only checks among active (non-deleted) categories — the global
            // query filter already excludes IsDeleted rows here, which is the
            // intended behavior: a deleted category's name can be reused.
            var query = DbSet.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
