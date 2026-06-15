using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        public SaleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sale>> GetAllWithDetailsAsync() =>
            await DbSet
                .Include(s => s.Customer)
                .Include(s => s.CashierUser)
                .Include(s => s.Items)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

        public async Task<Sale?> GetByIdWithDetailsAsync(int id) =>
            await DbSet
                .Include(s => s.Customer)
                .Include(s => s.CashierUser)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Batch)
                .Include(s => s.Returns)
                .FirstOrDefaultAsync(s => s.Id == id);
    }
}
