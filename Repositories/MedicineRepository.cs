using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class MedicineRepository : GenericRepository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Medicine>> GetAllWithDetailsAsync(bool includeDeleted = false)
        {
            var query = includeDeleted ? DbSet.IgnoreQueryFilters() : DbSet.AsQueryable();

            return await query
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Medicine?> GetByIdWithDetailsAsync(int id) =>
            await DbSet
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<Medicine?> GetByIdIncludingDeletedAsync(int id) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);

        public async Task<bool> NameStrengthExistsAsync(string name, string? strength, int? excludeId = null)
        {
            var normalizedStrength = strength?.Trim().ToLower() ?? string.Empty;

            var query = DbSet.Where(m =>
                m.Name.ToLower() == name.ToLower() &&
                (m.Strength ?? string.Empty).ToLower() == normalizedStrength);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> BarcodeExistsAsync(string barcode) =>
            await DbSet.IgnoreQueryFilters().AnyAsync(m => m.Barcode == barcode);
    }
}
