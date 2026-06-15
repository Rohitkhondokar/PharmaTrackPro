using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.PurchaseReturn;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class PurchaseReturnsController : Controller
    {
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly IPurchaseService _purchaseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseReturnsController(
            IPurchaseReturnService purchaseReturnService,
            IPurchaseService purchaseService,
            UserManager<ApplicationUser> userManager)
        {
            _purchaseReturnService = purchaseReturnService;
            _purchaseService = purchaseService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var returns = await _purchaseReturnService.GetAllAsync();

            var data = returns.Select(r => new
            {
                r.Id,
                r.PurchaseId,
                SupplierName = r.Purchase?.Supplier != null ? r.Purchase.Supplier.Name : "—",
                ReturnDate = r.ReturnDate.ToString("yyyy-MM-dd"),
                r.Reason,
                r.TotalRefundAmount,
                ItemCount = r.Items?.Count ?? 0
            });

            return Json(new { data });
        }

        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(int purchaseId)
        {
            var purchase = await _purchaseService.GetByIdAsync(purchaseId);
            if (purchase is null)
            {
                return NotFound();
            }

            if (purchase.Status != PurchaseStatus.Received)
            {
                TempData["ErrorMessage"] = "Only received purchases can have a return created.";
                return RedirectToAction("Details", "Purchases", new { id = purchaseId });
            }

            var batches = await _purchaseReturnService.GetReturnableBatchesAsync(purchaseId);

            var viewModel = new PurchaseReturnCreateViewModel
            {
                PurchaseId = purchaseId,
                SupplierName = purchase.Supplier?.Name ?? "—",
                AvailableBatches = batches.Select(b => new BatchRowViewModel
                {
                    BatchId = b.Id,
                    MedicineName = b.Medicine != null
                        ? b.Medicine.Name + (string.IsNullOrEmpty(b.Medicine.Strength) ? "" : $" ({b.Medicine.Strength})")
                        : "—",
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate,
                    QuantityRemaining = b.QuantityRemaining,
                    UnitCost = b.UnitCost
                }).ToList()
            };

            if (viewModel.AvailableBatches.Count == 0)
            {
                TempData["ErrorMessage"] = "There is no remaining stock on this purchase to return.";
                return RedirectToAction("Details", "Purchases", new { id = purchaseId });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(PurchaseReturnFormViewModel model)
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

            var result = await _purchaseReturnService.CreateAsync(model, userId);
            return Json(new { result.Success, result.Message, returnId = result.Data });
        }

        public async Task<IActionResult> Details(int id)
        {
            var purchaseReturn = await _purchaseReturnService.GetByIdAsync(id);
            if (purchaseReturn is null)
            {
                return NotFound();
            }

            return View(purchaseReturn);
        }
    }
}
