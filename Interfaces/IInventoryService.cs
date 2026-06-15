using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Inventory;

namespace PharmaTrackPro.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItemViewModel>> GetInventorySummaryAsync();

        /// <summary>Batches for a single medicine, ordered oldest-expiry-first.</summary>
        Task<IEnumerable<Batch>> GetBatchesForMedicineAsync(int medicineId);

        Task<ServiceResult> AdjustBatchQuantityAsync(int batchId, int newQuantity, string reason, string userId);
    }
}
