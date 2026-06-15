using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.Account;

namespace PharmaTrackPro.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLogService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || user.IsDeleted)
            {
                await _auditLogService.LogAsync("LoginFailed", "User", null, $"Login attempt with unrecognized email '{model.Email}'.", null, model.Email);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                await _auditLogService.LogAsync("LoginFailed", "User", user.Id, $"Login attempt on deactivated account '{model.Email}'.", user.Id, user.Email);
                ModelState.AddModelError(string.Empty, "This account has been deactivated. Contact your administrator.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in successfully.", model.Email);
                await _auditLogService.LogAsync("Login", "User", user.Id, $"User '{user.Email}' logged in.", user.Id, user.Email);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out due to failed login attempts.", model.Email);
                await _auditLogService.LogAsync("LoginFailed", "User", user.Id, $"Account locked out after repeated failed attempts for '{model.Email}'.", user.Id, model.Email);
                ModelState.AddModelError(string.Empty, "This account has been locked due to multiple failed login attempts. Try again in 15 minutes.");
                return View(model);
            }

            _logger.LogWarning("Invalid login attempt for {Email}.", model.Email);
            await _auditLogService.LogAsync("LoginFailed", "User", user.Id, $"Failed login attempt for '{model.Email}'.", user.Id, model.Email);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var email = User.Identity?.Name;
            await _auditLogService.LogAsync("Logout", "User", null, $"User '{email}' logged out.");
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {Email} logged out.", email);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
