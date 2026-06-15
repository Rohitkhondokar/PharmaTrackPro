using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Category;

namespace PharmaTrackPro.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync(bool includeDeleted = false);
        Task<Category?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CategoryFormViewModel model);
        Task<ServiceResult> UpdateAsync(CategoryFormViewModel model);
        Task<ServiceResult> SoftDeleteAsync(int id);
        Task<ServiceResult> RestoreAsync(int id);
    }
}
