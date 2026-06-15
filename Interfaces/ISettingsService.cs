using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Settings;

namespace PharmaTrackPro.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>Returns the single settings row, seeding it from appsettings.json defaults on first call.</summary>
        Task<PharmacySettings> GetSettingsAsync();

        Task<ServiceResult> UpdateSettingsAsync(PharmacySettingsFormViewModel model, string userId);
    }
}
