using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.Sales;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly ISettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesController(ISalesService salesService, ISettingsService settingsService, UserManager<ApplicationUser> userManager)
        {
            _salesService = salesService;
            _settingsService = settingsService;
            _userManager = userManager;
        }

        // The POS screen itself
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetFormOptions()
        {
            var data = await _salesService.GetPosDataAsync();
            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Cashier,Pharmacist")]
        public async Task<IActionResult> Checkout(SaleCheckoutViewModel model)
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

            var result = await _salesService.CheckoutAsync(model, userId);
            return Json(new { result.Success, result.Message, saleId = result.Data });
        }

        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSales()
        {
            var sales = await _salesService.GetAllAsync();

            var data = sales.Select(s => new
            {
                s.Id,
                CustomerName = s.Customer != null ? s.Customer.Name : "Walk-in",
                CashierName = s.CashierUser != null ? s.CashierUser.FullName : "—",
                SaleDate = s.SaleDate.ToString("MMM dd, yyyy HH:mm"),
                PaymentMethod = s.PaymentMethod.ToString(),
                s.TotalAmount,
                ItemCount = s.Items?.Count ?? 0
            });

            return Json(new { data });
        }

        public async Task<IActionResult> Details(int id)
        {
            var sale = await _salesService.GetByIdAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            return View(sale);
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var sale = await _salesService.GetByIdAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            ViewBag.PharmacySettings = await _settingsService.GetSettingsAsync();

            return View(sale);
        }
    }
}
