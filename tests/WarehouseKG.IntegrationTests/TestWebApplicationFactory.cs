using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WarehouseKG.Infrastructure.Identity;
using WarehouseKG.Infrastructure.Persistence;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WarehouseKgDbContext>>();
            services.RemoveAll<WarehouseKgDbContext>();

            services.AddDbContext<WarehouseKgDbContext>(options =>
                options.UseNpgsql(_connectionString));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WarehouseKgDbContext>();
            db.Database.Migrate();

            // Seed test admin user if not exists
            SeedTestUserAsync(scope).GetAwaiter().GetResult();
        });
    }

    private static async Task SeedTestUserAsync(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseKgDbContext>();

        var tenantId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

        // Ensure admin user exists
        var admin = await userManager.FindByNameAsync("admin");
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = "admin", Email = "admin@example.com", TenantId = tenantId };
            await userManager.CreateAsync(admin, "Admin1234!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Ensure Superadmin role exists and seed superadmin user
        if (!await roleManager.RoleExistsAsync("Superadmin"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "Superadmin" });

        var superadmin = await userManager.FindByNameAsync("superadmin");
        if (superadmin is null)
        {
            superadmin = new ApplicationUser { UserName = "superadmin", Email = "superadmin@example.com", TenantId = Guid.Empty };
            await userManager.CreateAsync(superadmin, "Super1234!");
            await userManager.AddToRoleAsync(superadmin, "Superadmin");
        }

        // Seed warehouse if none exist
        if (!db.Warehouses.IgnoreQueryFilters().Any(w => w.TenantId == tenantId))
        {
            db.Warehouses.Add(new WarehouseKG.Domain.Entities.Warehouse
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "DEPO-1", Name = "Test Warehouse", IsActive = true
            });
            db.Warehouses.Add(new WarehouseKG.Domain.Entities.Warehouse
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "DEPO-2", Name = "Second Warehouse", IsActive = true
            });
            await db.SaveChangesAsync();
        }

        // Seed supplier if none exist
        if (!db.Suppliers.IgnoreQueryFilters().Any(s => s.TenantId == tenantId))
        {
            db.Suppliers.Add(new WarehouseKG.Domain.Entities.Supplier
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "SUP-1", Name = "Test Supplier", IsActive = true
            });
            await db.SaveChangesAsync();
        }

        // Seed category if none exist
        Guid catId;
        var existingCat = db.ItemCategories.IgnoreQueryFilters()
            .FirstOrDefault(c => c.TenantId == tenantId && c.Code == "TEST");
        if (existingCat is null)
        {
            var cat = new WarehouseKG.Domain.Entities.ItemCategory
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "TEST", Name = "Test Category", IsActive = true
            };
            db.ItemCategories.Add(cat);
            await db.SaveChangesAsync();
            catId = cat.Id;
        }
        else
        {
            catId = existingCat.Id;
        }

        // Seed UOM if none exist
        Guid uomId;
        var existingUom = db.UnitsOfMeasure.IgnoreQueryFilters()
            .FirstOrDefault(u => u.TenantId == tenantId && u.Code == "PCS");
        if (existingUom is null)
        {
            var uom = new WarehouseKG.Domain.Entities.UnitOfMeasure
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "PCS", Name = "Pieces", IsActive = true
            };
            db.UnitsOfMeasure.Add(uom);
            await db.SaveChangesAsync();
            uomId = uom.Id;
        }
        else
        {
            uomId = existingUom.Id;
        }

        // Seed inventory item if none exist
        if (!db.InventoryItems.IgnoreQueryFilters().Any(i => i.TenantId == tenantId))
        {
            db.InventoryItems.Add(new WarehouseKG.Domain.Entities.InventoryItem
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Sku = "TEST-001", Name = "Test Item",
                CategoryId = catId, UnitOfMeasureId = uomId,
                QuantityOnHand = 0, IsActive = true
            });
            await db.SaveChangesAsync();
        }

        await db.SaveChangesAsync();

        // Seed tenant permissions for Admin role (all resources, full access)
        var resources = new[] { "warehouses", "inventory-items", "item-categories", "units-of-measure",
            "stock-receipts", "pick-orders", "pack-orders", "stock-transfers", "stock-adjustments", "stock-audits",
            "purchase-orders", "sales-orders", "reports", "suppliers", "stock-receipts-delete-completed", "add-items-back-in-time" };
        foreach (var res in resources)
        {
            if (!db.TenantPermissions.Any(p => p.TenantId == tenantId && p.RoleName == "Admin" && p.Resource == res))
            {
                db.TenantPermissions.Add(new WarehouseKG.Domain.Entities.TenantPermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RoleName = "Admin",
                    Resource = res,
                    CanRead = true, CanWrite = true, CanDelete = true,
                    MaxBackdateDays = 365
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
