using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.PurchaseReturn;

namespace PharmaTrackPro.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<IEnumerable<PurchaseReturn>> GetAllAsync();
        Task<PurchaseReturn?> GetByIdAsync(int id);

        /// <summary>Batches from a given purchase that still have stock remaining and can be returned.</summary>
        Task<IEnumerable<Batch>> GetReturnableBatchesAsync(int purchaseId);

        Task<ServiceResult<int>> CreateAsync(PurchaseReturnFormViewModel model, string createdByUserId);
    }
}
