using Microsoft.AspNetCore.Identity;
using CourtCaseTrackingSystem.Models;

public static class DbInitializer
{
    private const string AdminEmail = "henokayalew31@gmail.com";
    private const string AdminUsername = "admin";
    private const string AdminPassword = "Admin@123";
    private static readonly string[] Roles = { "Admin", "Judge", "Clerk", "Registrar" };

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Role '{roleName}' creation failed: {string.Join(", ", result.Errors)}");
                }
            }
        }
    }

    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminUser = await userManager.FindByNameAsync(AdminUsername);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminUsername,
                Email = AdminEmail,
                FirstName = "Henok",
                SecondName = "Ayalew",
                EmailConfirmed = true,
                Status = "Active"
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Admin user creation failed: {string.Join(", ", createResult.Errors)}");
            }

            // Refresh user after creation
            adminUser = await userManager.FindByNameAsync(AdminUsername)
                ?? throw new Exception("Admin user creation succeeded but user not found");
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Admin role assignment failed: {string.Join(", ", roleResult.Errors)}");
            }
        }
    }

    public static async Task SeedAllAsync(IServiceProvider serviceProvider)
    {
        await SeedRolesAsync(serviceProvider);
        await SeedAdminAsync(serviceProvider);
    }
}