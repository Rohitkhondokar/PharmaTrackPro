using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.SaleReturn;

namespace PharmaTrackPro.Interfaces
{
    public interface ISaleReturnService
    {
        Task<IEnumerable<SaleReturn>> GetAllAsync();
        Task<SaleReturn?> GetByIdAsync(int id);
        Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId);
        Task<ServiceResult<int>> CreateAsync(SaleReturnFormViewModel model, string createdByUserId);
    }
}
