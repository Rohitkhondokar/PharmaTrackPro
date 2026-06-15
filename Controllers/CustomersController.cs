using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Customer;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var customers = await _customerService.GetAllAsync(includeDeleted);

            var data = customers.Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.Email,
                DateOfBirth = c.DateOfBirth.HasValue ? c.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                c.IsActive,
                c.IsDeleted,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer is null)
            {
                return NotFound();
            }

            return Json(new
            {
                customer.Id,
                customer.Name,
                customer.Phone,
                customer.Email,
                customer.Address,
                DateOfBirth = customer.DateOfBirth.HasValue ? customer.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                customer.MedicalNotes,
                customer.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Cashier,Pharmacist")]
        public async Task<IActionResult> Create(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _customerService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Cashier,Pharmacist")]
        public async Task<IActionResult> Edit(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _customerService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _customerService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
