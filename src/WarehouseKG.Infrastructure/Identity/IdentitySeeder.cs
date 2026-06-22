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

        // Dispatcher: read all, write purchase-orders+sales-orders+invoices+suppliers+customers
        var dispatcherWrite = new[] { "purchase-orders", "sales-orders", "invoices", "suppliers", "customers" };
        var dispatcherDelete = new[] { "purchase-orders", "sales-orders", "invoices", "suppliers", "customers" };
        // Dispatcher also manages fleet
        var fleetResources = new[] { "vehicles", "vehicle-types", "maintenance", "insurance", "inspections" };
        dispatcherWrite = dispatcherWrite.Concat(fleetResources).ToArray();
        dispatcherDelete = dispatcherDelete.Concat(fleetResources).ToArray();
        await SeedPermissionRows(db, tenantId, Roles.Dispatcher, dispatcherWrite, dispatcherDelete, ct);

        // HR: read all, write personnel resources, delete personnel resources
        var personnelResources = new[] { "employees", "positions", "departments", "shifts", "attendance" };
        await SeedPermissionRows(db, tenantId, Roles.HR, personnelResources, personnelResources, ct);

        // Driver: read delivery resources, write delivery-routes and delivery-stops, no delete
        var driverWrite = new[] { "delivery-routes", "delivery-stops" };
        var driverReadOnly = new[] { "delivery-shipments", "geofences", "sales-orders", "invoices", "customers" };
        await SeedPermissionRows(db, tenantId, Roles.Driver, driverWrite.Concat(driverReadOnly).ToArray(), Array.Empty<string>(), ct);

        // Preseller: read customers, inventory, warehouses, reports, payment-types; write pre-orders
        var presellerWrite = new[] { "pre-orders" };
        var presellerReadOnly = new[] { "customers", "inventory-items", "warehouses", "reports", "payment-types" };
        await SeedPermissionRows(db, tenantId, Roles.Preseller, presellerWrite.Concat(presellerReadOnly).ToArray(), Array.Empty<string>(), ct);
    }

    private static async Task SeedPermissionRows(
        Persistence.WarehouseKgDbContext db, Guid tenantId,
        string roleName, string[] writeResources, string[] deleteResources, CancellationToken ct)
    {
        // Fast check: if ANY permission row exists for this role+tenant, skip entirely
        // Ignore query filters because the global tenant filter may use an unset tenant during startup
        var anyExist = await db.TenantPermissions
            .IgnoreQueryFilters()
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

    /// <summary>
    /// Seeds default payment types (Наличные, Безналичный, Карта, Кредит) for all tenants.
    /// </summary>
    public static async Task SeedPaymentTypesAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<Persistence.WarehouseKgDbContext>();

        var allTenantIds = await db.PaymentTypes
            .IgnoreQueryFilters()
            .Select(p => p.TenantId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var tenantIds = allTenantIds.Count > 0
            ? allTenantIds
            : new List<Guid> { Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff") };

        var defaults = new (string Code, string Name)[]
        {
            ("CASH", "Наличные"),
            ("BANK", "Безналичный"),
            ("CARD", "Карта"),
            ("CREDIT", "Кредит"),
        };

        foreach (var tenantId in tenantIds)
        {
            foreach (var (code, name) in defaults)
            {
                var exists = await db.PaymentTypes
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.TenantId == tenantId && p.Code == code, cancellationToken);

                if (!exists)
                {
                    db.PaymentTypes.Add(new Domain.Entities.PaymentType
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Code = code,
                        Name = name,
                        IsActive = true,
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
