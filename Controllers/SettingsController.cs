using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.Settings;

namespace PharmaTrackPro.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(ISettingsService settingsService, UserManager<ApplicationUser> userManager)
        {
            _settingsService = settingsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();

            var model = new PharmacySettingsFormViewModel
            {
                PharmacyName = settings.PharmacyName,
                Address = settings.Address,
                Phone = settings.Phone,
                Email = settings.Email,
                LowStockThreshold = settings.LowStockThreshold,
                NearExpiryDaysThreshold = settings.NearExpiryDaysThreshold,
                CurrencySymbol = settings.CurrencySymbol
            };

            ViewBag.UpdatedAt = settings.UpdatedAt;
            ViewBag.UpdatedByName = settings.UpdatedByUser?.FullName;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PharmacySettingsFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Could not identify the current user." });
            }

            var result = await _settingsService.UpdateSettingsAsync(model, userId);
            return Json(new { result.Success, result.Message });
        }
    }
}
