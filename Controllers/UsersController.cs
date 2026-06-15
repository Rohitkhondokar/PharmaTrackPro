using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.User;

namespace PharmaTrackPro.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserManagementService userManagementService, UserManager<ApplicationUser> userManager)
        {
            _userManagementService = userManagementService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var users = await _userManagementService.GetAllAsync(includeDeleted);
            return Json(new { data = users });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManagementService.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            return Json(user);
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            return Json(SeedData.Roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _userManagementService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var currentUserId = _userManager.GetUserId(User);
            if (model.Id == currentUserId && !model.IsActive)
            {
                return Json(new { success = false, message = "You cannot deactivate your own account." });
            }

            var result = await _userManagementService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                return Json(new { success = false, message = "You cannot deactivate your own account." });
            }

            var result = await _userManagementService.ToggleActiveAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                return Json(new { success = false, message = "You cannot delete your own account." });
            }

            var result = await _userManagementService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _userManagementService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Enter a password of at least 8 characters." });
            }

            var result = await _userManagementService.ResetPasswordAsync(model.UserId, model.NewPassword);
            return Json(new { result.Success, result.Message });
        }
    }
}
