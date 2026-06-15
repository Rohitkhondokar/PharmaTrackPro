using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.Purchase;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ISupplierService _supplierService;
        private readonly IMedicineService _medicineService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesController(
            IPurchaseService purchaseService,
            ISupplierService supplierService,
            IMedicineService medicineService,
            UserManager<ApplicationUser> userManager)
        {
            _purchaseService = purchaseService;
            _supplierService = supplierService;
            _medicineService = medicineService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var purchases = await _purchaseService.GetAllAsync();

            var data = purchases.Select(p => new
            {
                p.Id,
                SupplierName = p.Supplier != null ? p.Supplier.Name : "—",
                p.InvoiceNumber,
                PurchaseDate = p.PurchaseDate.ToString("yyyy-MM-dd"),
                Status = p.Status.ToString(),
                p.TotalAmount,
                ItemCount = p.Items?.Count ?? 0
            });

            return Json(new { data });
        }

        [Authorize(Roles = "Administrator,StoreManager")]
        public IActionResult Create()
        {
            return View(new PurchaseFormViewModel { PurchaseDate = DateTime.Today });
        }

        [HttpGet]
        public async Task<IActionResult> GetFormOptions()
        {
            var suppliers = await _supplierService.GetAllAsync();
            var medicines = await _medicineService.GetAllAsync();

            return Json(new
            {
                suppliers = suppliers.Where(s => s.IsActive).Select(s => new { s.Id, s.Name }),
                medicines = medicines.Where(m => m.IsActive).Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Strength,
                    m.PurchasePrice
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(PurchaseFormViewModel model)
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

            var result = await _purchaseService.CreateAsync(model, userId);
            return Json(new { result.Success, result.Message, purchaseId = result.Data });
        }

        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _purchaseService.GetByIdAsync(id);
            if (purchase is null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Receive(int id)
        {
            var result = await _purchaseService.ReceiveAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _purchaseService.CancelAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
