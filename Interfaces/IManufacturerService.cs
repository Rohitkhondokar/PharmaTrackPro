using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Manufacturer;

namespace PharmaTrackPro.Interfaces
{
    public interface IManufacturerService
    {
        Task<IEnumerable<Manufacturer>> GetAllAsync(bool includeDeleted = false);
        Task<Manufacturer?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(ManufacturerFormViewModel model);
        Task<ServiceResult> UpdateAsync(ManufacturerFormViewModel model);
        Task<ServiceResult> SoftDeleteAsync(int id);
        Task<ServiceResult> RestoreAsync(int id);
    }
}
