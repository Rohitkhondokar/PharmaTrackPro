using Microsoft.AspNetCore.Identity;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Data
{
    /// <summary>
    /// Seeds default roles and a default Administrator account on startup.
    /// Called once from Program.cs after the app is built.
    /// </summary>
    public static class SeedData
    {
        public static readonly string[] Roles =
        {
            "Administrator",
            "Pharmacist",
            "Cashier",
            "StoreManager"
        };

        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed roles
            foreach (var roleName in Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed default administrator
            var adminEmail = configuration["AppSettings:DefaultAdminEmail"] ?? "admin@pharmatrackpro.local";
            var adminPassword = configuration["AppSettings:DefaultAdminPassword"] ?? "Admin@12345";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin is null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to seed default administrator: {errors}");
                }
            }
        }
    }
}
