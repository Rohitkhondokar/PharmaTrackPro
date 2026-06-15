using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Sales;

namespace PharmaTrackPro.Interfaces
{
    public interface ISalesService
    {
        Task<PosDataViewModel> GetPosDataAsync();
        Task<ServiceResult<int>> CheckoutAsync(SaleCheckoutViewModel model, string cashierUserId);
        Task<IEnumerable<Sale>> GetAllAsync();
        Task<Sale?> GetByIdAsync(int id);
    }
}
