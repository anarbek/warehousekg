using Microsoft.AspNetCore.Identity;
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
}
