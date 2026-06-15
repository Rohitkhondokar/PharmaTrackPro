using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InventoryController(IInventoryService inventoryService, UserManager<ApplicationUser> userManager)
        {
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var summary = await _inventoryService.GetInventorySummaryAsync();

            var data = summary.Select(i => new
            {
                i.MedicineId,
                i.Name,
                i.Strength,
                i.CategoryName,
                i.ManufacturerName,
                i.UnitOfMeasure,
                i.TotalQuantity,
                i.SellingPrice,
                i.IsLowStock,
                i.HasExpiredBatches,
                i.HasNearExpiryBatches
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetBatches(int medicineId)
        {
            var batches = await _inventoryService.GetBatchesForMedicineAsync(medicineId);
            var today = DateTime.Today;

            var data = batches.Select(b => new
            {
                b.Id,
                b.BatchNumber,
                ExpiryDate = b.ExpiryDate.ToString("yyyy-MM-dd"),
                IsExpired = b.ExpiryDate.Date < today,
                b.QuantityReceived,
                b.QuantityRemaining,
                b.UnitCost
            });

            return Json(new { data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Adjust(int batchId, int newQuantity, string reason)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Could not identify the current user." });
            }

            var result = await _inventoryService.AdjustBatchQuantityAsync(batchId, newQuantity, reason, userId);
            return Json(new { result.Success, result.Message });
        }
    }
}
