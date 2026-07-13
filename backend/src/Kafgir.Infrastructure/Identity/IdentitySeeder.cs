using Kafgir.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Kafgir.Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string DevelopmentAdminUsername = "admin";
    private const string DevelopmentAdminPassword = "Admin@123456";

    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<int>(roleName)), $"create role {roleName}");
            }
        }

        var customerUsers = await userManager.Users
            .Where(user => user.CustomerProfile != null)
            .ToListAsync();
        foreach (var customerUser in customerUsers)
        {
            if (!await userManager.IsInRoleAsync(customerUser, AppRoles.Customer))
            {
                EnsureSucceeded(
                    await userManager.AddToRoleAsync(customerUser, AppRoles.Customer),
                    "assign Customer role to migrated user");
            }
        }

        var admin = await userManager.FindByNameAsync(DevelopmentAdminUsername);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DevelopmentAdminUsername,
                FullName = "مدیر کفگیر",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            EnsureSucceeded(
                await userManager.CreateAsync(admin, DevelopmentAdminPassword),
                "create development admin");
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Owner))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(admin, AppRoles.Owner), "assign Owner role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
