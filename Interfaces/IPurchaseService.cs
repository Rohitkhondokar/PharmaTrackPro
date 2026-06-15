using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Purchase;

namespace PharmaTrackPro.Interfaces
{
    public interface IPurchaseService
    {
        Task<IEnumerable<Purchase>> GetAllAsync();
        Task<Purchase?> GetByIdAsync(int id);
        Task<ServiceResult<int>> CreateAsync(PurchaseFormViewModel model, string createdByUserId);
        Task<ServiceResult> ReceiveAsync(int id);
        Task<ServiceResult> CancelAsync(int id);
    }
}
