using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.ViewModels.User;

namespace PharmaTrackPro.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAuditLogService auditLogService,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _context = context;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<UserListItemViewModel>> GetAllAsync(bool includeDeleted = false)
        {
            var query = includeDeleted ? _context.Users.IgnoreQueryFilters() : _context.Users.AsQueryable();
            var users = await query.OrderBy(u => u.FullName).ToListAsync();

            var result = new List<UserListItemViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "—",
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }

            return result;
        }

        public async Task<UserFormViewModel?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive
            };
        }

        public async Task<ServiceResult> CreateAsync(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
            {
                return ServiceResult.Fail("A password of at least 8 characters is required to create an account.");
            }

            if (!SeedData.Roles.Contains(model.Role))
            {
                return ServiceResult.Fail("Select a valid role.");
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
            {
                return ServiceResult.Fail($"An account with email \"{model.Email}\" already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                EmailConfirmed = true,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return ServiceResult.Fail(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            _logger.LogInformation("User account '{Email}' created with role '{Role}'.", user.Email, model.Role);
            await _auditLogService.LogAsync("UserCreated", "User", user.Id, $"User account '{user.Email}' created with role '{model.Role}'.");

            return ServiceResult.Ok("User created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(UserFormViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return ServiceResult.Fail("User not found.");
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            if (!SeedData.Roles.Contains(model.Role))
            {
                return ServiceResult.Fail("Select a valid role.");
            }

            var existingWithEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
            {
                return ServiceResult.Fail($"An account with email \"{model.Email}\" already exists.");
            }

            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.UserName = model.Email.Trim();
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ServiceResult.Fail(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            _logger.LogInformation("User account '{Email}' updated (role: {Role}, active: {IsActive}).", user.Email, model.Role, model.IsActive);
            await _auditLogService.LogAsync("UserUpdated", "User", user.Id, $"User account '{user.Email}' updated (role: {model.Role}, active: {model.IsActive}).");

            return ServiceResult.Ok("User updated successfully.");
        }

        public async Task<ServiceResult> ToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var action = user.IsActive ? "UserActivated" : "UserDeactivated";
            var message = $"User account '{user.Email}' {(user.IsActive ? "activated" : "deactivated")}.";
            await _auditLogService.LogAsync(action, "User", user.Id, message);

            return ServiceResult.Ok(user.IsActive ? "User activated." : "User deactivated.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            user.IsDeleted = true;
            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            await _auditLogService.LogAsync("UserDeleted", "User", user.Id, $"User account '{user.Email}' deleted.");

            return ServiceResult.Ok("User deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(string id)
        {
            // UserManager.FindByIdAsync respects the IsDeleted query filter, so a
            // soft-deleted user must be fetched directly with IgnoreQueryFilters.
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            user.IsDeleted = false;
            user.IsActive = true;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync("UserRestored", "User", user.Id, $"User account '{user.Email}' restored.");

            return ServiceResult.Ok("User restored successfully.");
        }

        public async Task<ServiceResult> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                return ServiceResult.Fail(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _auditLogService.LogAsync("PasswordReset", "User", user.Id, $"Password reset for user '{user.Email}' by an administrator.");

            return ServiceResult.Ok("Password reset successfully.");
        }
    }
}
