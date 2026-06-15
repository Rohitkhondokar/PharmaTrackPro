using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class PurchaseReturnRepository : GenericRepository<PurchaseReturn>, IPurchaseReturnRepository
    {
        public PurchaseReturnRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseReturn>> GetAllWithDetailsAsync() =>
            await DbSet
                .Include(r => r.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();

        public async Task<PurchaseReturn?> GetByIdWithDetailsAsync(int id) =>
            await DbSet
                .Include(r => r.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Batch)
                        .ThenInclude(b => b!.Medicine)
                .FirstOrDefaultAsync(r => r.Id == id);
    }
}
