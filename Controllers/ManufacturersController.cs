using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Manufacturer;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class ManufacturersController : Controller
    {
        private readonly IManufacturerService _manufacturerService;

        public ManufacturersController(IManufacturerService manufacturerService)
        {
            _manufacturerService = manufacturerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var manufacturers = await _manufacturerService.GetAllAsync(includeDeleted);

            var data = manufacturers.Select(m => new
            {
                m.Id,
                m.Name,
                m.ContactPerson,
                m.Phone,
                m.Email,
                m.IsActive,
                m.IsDeleted,
                CreatedAt = m.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _manufacturerService.GetByIdAsync(id);
            if (manufacturer is null)
            {
                return NotFound();
            }

            return Json(new
            {
                manufacturer.Id,
                manufacturer.Name,
                manufacturer.ContactPerson,
                manufacturer.Phone,
                manufacturer.Email,
                manufacturer.Address,
                manufacturer.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(ManufacturerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _manufacturerService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Edit(ManufacturerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _manufacturerService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _manufacturerService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _manufacturerService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
