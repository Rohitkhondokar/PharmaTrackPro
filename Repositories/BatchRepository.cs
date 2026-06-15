using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class BatchRepository : GenericRepository<Batch>, IBatchRepository
    {
        public BatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Batch>> GetByPurchaseIdAsync(int purchaseId) =>
            await DbSet
                .Include(b => b.Medicine)
                .Where(b => b.PurchaseItem != null && b.PurchaseItem.PurchaseId == purchaseId)
                .OrderBy(b => b.Medicine!.Name)
                .ToListAsync();

        public async Task<IEnumerable<Batch>> GetAllWithMedicineAsync() =>
            await DbSet
                .Include(b => b.Medicine)
                .ToListAsync();

        public async Task<IEnumerable<Batch>> GetByMedicineIdAsync(int medicineId) =>
            await DbSet
                .Where(b => b.MedicineId == medicineId)
                .OrderBy(b => b.ExpiryDate) // FEFO — First-Expire-First-Out
                .ToListAsync();
    }
}
