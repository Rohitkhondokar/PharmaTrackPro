using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Purchase>> GetAllWithDetailsAsync() =>
            await DbSet
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

        public async Task<Purchase?> GetByIdWithDetailsAsync(int id) =>
            await DbSet
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.Returns)
                .FirstOrDefaultAsync(p => p.Id == id);
    }
}
