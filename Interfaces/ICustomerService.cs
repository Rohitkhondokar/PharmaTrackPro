using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Customer;

namespace PharmaTrackPro.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync(bool includeDeleted = false);
        Task<Customer?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CustomerFormViewModel model);
        Task<ServiceResult> UpdateAsync(CustomerFormViewModel model);
        Task<ServiceResult> SoftDeleteAsync(int id);
        Task<ServiceResult> RestoreAsync(int id);
    }
}
