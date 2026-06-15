using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.SaleReturn;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class SaleReturnsController : Controller
    {
        private readonly ISaleReturnService _saleReturnService;
        private readonly ISalesService _salesService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SaleReturnsController(
            ISaleReturnService saleReturnService,
            ISalesService salesService,
            UserManager<ApplicationUser> userManager)
        {
            _saleReturnService = saleReturnService;
            _salesService = salesService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var returns = await _saleReturnService.GetAllAsync();

            var data = returns.Select(r => new
            {
                r.Id,
                r.SaleId,
                CustomerName = r.Sale?.Customer != null ? r.Sale.Customer.Name : "Walk-in",
                ReturnDate = r.ReturnDate.ToString("yyyy-MM-dd"),
                r.Reason,
                r.TotalRefundAmount,
                ItemCount = r.Items?.Count ?? 0
            });

            return Json(new { data });
        }

        [Authorize(Roles = "Administrator,StoreManager,Pharmacist")]
        public async Task<IActionResult> Create(int saleId)
        {
            var sale = await _salesService.GetByIdAsync(saleId);
            if (sale is null)
            {
                return NotFound();
            }

            var returnedQuantities = await _saleReturnService.GetReturnedQuantitiesForSaleAsync(saleId);

            var rows = sale.Items.Select(item => new SaleItemRowViewModel
            {
                SaleItemId = item.Id,
                MedicineName = item.Medicine != null
                    ? item.Medicine.Name + (string.IsNullOrEmpty(item.Medicine.Strength) ? "" : $" ({item.Medicine.Strength})")
                    : "—",
                BatchNumber = item.Batch?.BatchNumber ?? "—",
                OriginalQuantity = item.Quantity,
                AlreadyReturned = returnedQuantities.TryGetValue(item.Id, out var r) ? r : 0,
                UnitPrice = item.UnitPrice
            })
            .Where(row => row.Returnable > 0)
            .ToList();

            if (rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Everything on this sale has already been returned.";
                return RedirectToAction("Details", "Sales", new { id = saleId });
            }

            var viewModel = new SaleReturnCreateViewModel
            {
                SaleId = saleId,
                CustomerName = sale.Customer?.Name ?? "Walk-in Customer",
                ReturnableItems = rows
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Pharmacist")]
        public async Task<IActionResult> Create(SaleReturnFormViewModel model)
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

            var result = await _saleReturnService.CreateAsync(model, userId);
            return Json(new { result.Success, result.Message, returnId = result.Data });
        }

        public async Task<IActionResult> Details(int id)
        {
            var saleReturn = await _saleReturnService.GetByIdAsync(id);
            if (saleReturn is null)
            {
                return NotFound();
            }

            return View(saleReturn);
        }
    }
}
