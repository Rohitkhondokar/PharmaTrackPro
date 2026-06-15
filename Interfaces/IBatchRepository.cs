using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IBatchRepository : IGenericRepository<Batch>
    {
        /// <summary>All batches created from a given purchase, with Medicine loaded.</summary>
        Task<IEnumerable<Batch>> GetByPurchaseIdAsync(int purchaseId);

        /// <summary>Every batch in the system, with Medicine loaded — used to build the inventory summary.</summary>
        Task<IEnumerable<Batch>> GetAllWithMedicineAsync();

        /// <summary>Batches for a single medicine, ordered oldest-expiry-first (FEFO).</summary>
        Task<IEnumerable<Batch>> GetByMedicineIdAsync(int medicineId);
    }
}
