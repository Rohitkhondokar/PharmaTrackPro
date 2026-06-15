using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Medicine;

namespace PharmaTrackPro.Interfaces
{
    public interface IMedicineService
    {
        Task<IEnumerable<Medicine>> GetAllAsync(bool includeDeleted = false);
        Task<Medicine?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(MedicineFormViewModel model);
        Task<ServiceResult> UpdateAsync(MedicineFormViewModel model);
        Task<ServiceResult> SoftDeleteAsync(int id);
        Task<ServiceResult> RestoreAsync(int id);
    }
}
