using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class ExpiryController : Controller
    {
        private readonly IExpiryService _expiryService;
        private readonly IInventoryService _inventoryService;
        private readonly ISettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpiryController(
            IExpiryService expiryService,
            IInventoryService inventoryService,
            ISettingsService settingsService,
            UserManager<ApplicationUser> userManager)
        {
            _expiryService = expiryService;
            _inventoryService = inventoryService;
            _settingsService = settingsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();
            ViewBag.NearExpiryDays = settings.NearExpiryDaysThreshold;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var report = await _expiryService.GetExpiryReportAsync();

            var data = report.Select(b => new
            {
                b.BatchId,
                b.MedicineId,
                b.MedicineName,
                b.Strength,
                b.BatchNumber,
                ExpiryDate = b.ExpiryDate.ToString("yyyy-MM-dd"),
                b.DaysUntilExpiry,
                b.QuantityRemaining,
                b.UnitCost,
                b.IsExpired
            });

            return Json(new { data });
        }

        /// <summary>Writes off an expired batch by zeroing its remaining quantity, using the same adjustment mechanism as Inventory.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> WriteOff(int batchId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Could not identify the current user." });
            }

            var result = await _inventoryService.AdjustBatchQuantityAsync(batchId, 0, "Expired — written off via Expiry Management", userId);
            return Json(new { result.Success, result.Message });
        }
    }
}
