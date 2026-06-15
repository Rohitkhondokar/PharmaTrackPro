using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Supplier;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var suppliers = await _supplierService.GetAllAsync(includeDeleted);

            var data = suppliers.Select(s => new
            {
                s.Id,
                s.Name,
                s.ContactPerson,
                s.Phone,
                s.Email,
                s.LicenseNumber,
                s.PaymentTerms,
                s.IsActive,
                s.IsDeleted,
                CreatedAt = s.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier is null)
            {
                return NotFound();
            }

            return Json(new
            {
                supplier.Id,
                supplier.Name,
                supplier.ContactPerson,
                supplier.Phone,
                supplier.Email,
                supplier.Address,
                supplier.LicenseNumber,
                supplier.PaymentTerms,
                supplier.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(SupplierFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _supplierService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Edit(SupplierFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _supplierService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supplierService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _supplierService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
