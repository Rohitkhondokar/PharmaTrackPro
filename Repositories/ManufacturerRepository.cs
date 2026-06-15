using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class ManufacturerRepository : GenericRepository<Manufacturer>, IManufacturerRepository
    {
        public ManufacturerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Manufacturer?> GetByIdIncludingDeletedAsync(int id) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);

        public async Task<IEnumerable<Manufacturer>> GetAllIncludingDeletedAsync() =>
            await DbSet.IgnoreQueryFilters().OrderBy(m => m.Name).ToListAsync();

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = DbSet.Where(m => m.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
