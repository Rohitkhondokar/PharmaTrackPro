using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Supplier?> GetByIdIncludingDeletedAsync(int id) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<Supplier>> GetAllIncludingDeletedAsync() =>
            await DbSet.IgnoreQueryFilters().OrderBy(s => s.Name).ToListAsync();

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = DbSet.Where(s => s.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
