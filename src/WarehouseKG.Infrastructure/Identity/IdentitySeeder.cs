using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Infrastructure.Identity;

public static class IdentitySeeder
{
    /// <summary>Ensures the four canonical roles exist. Safe to call on every startup.</summary>
    public static async Task SeedRolesAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }
    }

    /// <summary>
    /// Ensures the first registered user has Admin role (for development convenience).
    /// </summary>
    public static async Task SeedAdminUserAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await userManager.Users.FirstOrDefaultAsync(cancellationToken);
        if (admin is null) return;
        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
