using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class SaleReturnRepository : GenericRepository<SaleReturn>, ISaleReturnRepository
    {
        public SaleReturnRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SaleReturn>> GetAllWithDetailsAsync() =>
            await DbSet
                .Include(r => r.Sale)
                    .ThenInclude(s => s!.Customer)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();

        public async Task<SaleReturn?> GetByIdWithDetailsAsync(int id) =>
            await DbSet
                .Include(r => r.Sale)
                    .ThenInclude(s => s!.Customer)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SaleItem)
                        .ThenInclude(si => si!.Medicine)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId) =>
            await Context.Set<SaleReturnItem>()
                .Where(i => i.SaleReturn != null && i.SaleReturn.SaleId == saleId)
                .GroupBy(i => i.SaleItemId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(i => i.Quantity));
    }
}
