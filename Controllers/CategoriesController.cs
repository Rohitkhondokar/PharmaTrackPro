using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Category;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Any authenticated user can view categories (needed later when
        // Pharmacist/Cashier pick a category while working with Medicines).
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var categories = await _categoryService.GetAllAsync(includeDeleted);

            var data = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.IsDeleted,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category is null)
            {
                return NotFound();
            }

            return Json(new
            {
                category.Id,
                category.Name,
                category.Description,
                category.IsActive
            });
        }

        // Category management (create/edit/delete/restore) is limited to
        // Administrator and Store Manager — Pharmacist/Cashier get read-only access.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _categoryService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _categoryService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _categoryService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
