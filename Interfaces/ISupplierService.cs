using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Supplier;

namespace PharmaTrackPro.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync(bool includeDeleted = false);
        Task<Supplier?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(SupplierFormViewModel model);
        Task<ServiceResult> UpdateAsync(SupplierFormViewModel model);
        Task<ServiceResult> SoftDeleteAsync(int id);
        Task<ServiceResult> RestoreAsync(int id);
    }
}
