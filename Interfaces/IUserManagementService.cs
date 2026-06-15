using PharmaTrackPro.Helpers;
using PharmaTrackPro.ViewModels.User;

namespace PharmaTrackPro.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserListItemViewModel>> GetAllAsync(bool includeDeleted = false);
        Task<UserFormViewModel?> GetByIdAsync(string id);
        Task<ServiceResult> CreateAsync(UserFormViewModel model);
        Task<ServiceResult> UpdateAsync(UserFormViewModel model);
        Task<ServiceResult> ToggleActiveAsync(string id);
        Task<ServiceResult> SoftDeleteAsync(string id);
        Task<ServiceResult> RestoreAsync(string id);
        Task<ServiceResult> ResetPasswordAsync(string userId, string newPassword);
    }
}
