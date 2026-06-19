using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Identity;
using WarehouseKG.Infrastructure.Persistence;

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
    /// Seeds default TenantPermission rows for roles that need specific write access
    /// (Auditor, Dispatcher, HR). Called once at startup. Admin can customize via /admin/permissions.
    /// </summary>
    public static async Task SeedDefaultPermissionsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<Persistence.WarehouseKgDbContext>();

        // Use a well-known tenant ID for seeding (same as TestWebApplicationFactory pattern).
        // In production, this runs for each tenant via the tenant context.
        var allTenantIds = await db.TenantPermissions
            .Select(p => p.TenantId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // If no tenants exist with permissions yet, seed for the default tenant
        var tenantIds = allTenantIds.Count > 0
            ? allTenantIds
            : new List<Guid> { Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff") };

        foreach (var tenantId in tenantIds)
        {
            await SeedPermissionsForTenant(db, tenantId, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPermissionsForTenant(
        Persistence.WarehouseKgDbContext db, Guid tenantId, CancellationToken ct)
    {
        // Auditor: read all, write adjustments+audits, delete audits only
        var auditorWrite = new[] { "stock-adjustments", "stock-audits" };
        var auditorDelete = new[] { "stock-audits" };
        await SeedPermissionRows(db, tenantId, Roles.Auditor, auditorWrite, auditorDelete, ct);

        // Dispatcher: read all, write purchase-orders+sales-orders+suppliers+customers, delete nothing
        var dispatcherWrite = new[] { "purchase-orders", "sales-orders", "suppliers", "customers" };
        var dispatcherDelete = new[] { "purchase-orders", "sales-orders", "suppliers", "customers" };
        // Dispatcher also manages fleet
        var fleetResources = new[] { "vehicles", "vehicle-types", "maintenance", "insurance", "inspections" };
        dispatcherWrite = dispatcherWrite.Concat(fleetResources).ToArray();
        dispatcherDelete = dispatcherDelete.Concat(fleetResources).ToArray();
        await SeedPermissionRows(db, tenantId, Roles.Dispatcher, dispatcherWrite, dispatcherDelete, ct);

        // HR: read all, write personnel resources, delete personnel resources
        var personnelResources = new[] { "employees", "positions", "departments", "shifts", "attendance" };
        await SeedPermissionRows(db, tenantId, Roles.HR, personnelResources, personnelResources, ct);
    }

    private static async Task SeedPermissionRows(
        Persistence.WarehouseKgDbContext db, Guid tenantId,
        string roleName, string[] writeResources, string[] deleteResources, CancellationToken ct)
    {
        // Fast check: if ANY permission row exists for this role+tenant, skip entirely
        var anyExist = await db.TenantPermissions
            .AnyAsync(p => p.TenantId == tenantId && p.RoleName == roleName, ct);

        if (anyExist) return;

        foreach (var resource in Domain.Identity.Resources.All)
        {
            // Skip special resources that don't use standard CRUD permissions
            if (resource is "add-items-back-in-time" or "stock-receipts-delete-completed")
                continue;

            db.TenantPermissions.Add(new Domain.Entities.TenantPermission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleName = roleName,
                Resource = resource,
                CanRead = true,
                CanWrite = writeResources.Contains(resource),
                CanDelete = deleteResources.Contains(resource)
            });
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
