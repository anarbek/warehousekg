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

        // Seed warehouse if none exist
        if (!db.Warehouses.Any())
        {
            db.Warehouses.Add(new WarehouseKG.Domain.Entities.Warehouse
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "DEPO-1", Name = "Test Warehouse", IsActive = true
            });
        }

        // Seed inventory item if none exist
        if (!db.InventoryItems.Any())
        {
            // Seed category
            var cat = new WarehouseKG.Domain.Entities.ItemCategory
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "TEST", Name = "Test Category", IsActive = true
            };
            db.ItemCategories.Add(cat);

            // Seed UOM
            var uom = new WarehouseKG.Domain.Entities.UnitOfMeasure
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Code = "PCS", Name = "Pieces", IsActive = true
            };
            db.UnitsOfMeasure.Add(uom);

            await db.SaveChangesAsync();

            db.InventoryItems.Add(new WarehouseKG.Domain.Entities.InventoryItem
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Sku = "TEST-001", Name = "Test Item",
                CategoryId = cat.Id, UnitOfMeasureId = uom.Id,
                QuantityOnHand = 0, IsActive = true
            });
        }

        await db.SaveChangesAsync();

        // Seed tenant permissions for Admin role (all resources, full access)
        var resources = new[] { "warehouses", "inventory-items", "item-categories", "units-of-measure",
            "stock-receipts", "pick-orders", "pack-orders", "stock-transfers", "stock-adjustments", "stock-audits",
            "purchase-orders", "sales-orders", "reports", "stock-receipts-delete-completed", "add-items-back-in-time" };
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
                    CanRead = true, CanWrite = true, CanDelete = true
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
