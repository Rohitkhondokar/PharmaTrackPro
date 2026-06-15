using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Settings;

namespace PharmaTrackPro.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IGenericRepository<PharmacySettings> _settingsRepository;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;

        public SettingsService(
            IGenericRepository<PharmacySettings> settingsRepository,
            IConfiguration configuration,
            IAuditLogService auditLogService)
        {
            _settingsRepository = settingsRepository;
            _configuration = configuration;
            _auditLogService = auditLogService;
        }

        public async Task<PharmacySettings> GetSettingsAsync()
        {
            var existing = (await _settingsRepository.GetAllAsync()).FirstOrDefault();
            if (existing is not null)
            {
                return existing;
            }

            // First run — seed from the appsettings.json defaults that have been
            // in use since Step 8/10; from here on, this row is the source of truth.
            var settings = new PharmacySettings
            {
                PharmacyName = _configuration["AppSettings:ApplicationName"] ?? "PharmaTrack Pro",
                LowStockThreshold = _configuration.GetValue("AppSettings:LowStockThreshold", 20),
                NearExpiryDaysThreshold = _configuration.GetValue("AppSettings:NearExpiryDaysThreshold", 90),
                CurrencySymbol = "$",
                UpdatedAt = DateTime.UtcNow
            };

            await _settingsRepository.AddAsync(settings);
            await _settingsRepository.SaveChangesAsync();

            return settings;
        }

        public async Task<ServiceResult> UpdateSettingsAsync(PharmacySettingsFormViewModel model, string userId)
        {
            var settings = await GetSettingsAsync();

            settings.PharmacyName = model.PharmacyName.Trim();
            settings.Address = model.Address?.Trim();
            settings.Phone = model.Phone?.Trim();
            settings.Email = model.Email?.Trim();
            settings.LowStockThreshold = model.LowStockThreshold;
            settings.NearExpiryDaysThreshold = model.NearExpiryDaysThreshold;
            settings.CurrencySymbol = model.CurrencySymbol.Trim();
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedByUserId = userId;

            _settingsRepository.Update(settings);
            await _settingsRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "SettingsUpdated",
                "PharmacySettings",
                settings.Id.ToString(),
                $"System settings updated (Low Stock Threshold: {settings.LowStockThreshold}, Near Expiry Threshold: {settings.NearExpiryDaysThreshold} days).");

            return ServiceResult.Ok("Settings saved successfully.");
        }
    }
}
