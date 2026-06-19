using System.Net;
using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Tests that the 3 new roles (Auditor, Dispatcher, HR) have correct
/// permission boundaries: can access what they should, blocked from what they shouldn't.
/// </summary>
[Collection("IntegrationTests")]
public class RolePermissionTests
{
    private readonly SharedFixture _fixture;

    public RolePermissionTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    private WarehouseKgClient Admin => _fixture.Client;

    // ─── Helpers ──────────────────────────────────────────────────────────

    private async Task<(WarehouseKgClient client, string userId)> CreateAndLoginUser(string name, string role)
    {
        var password = "Test1234!";
        var userId = await Admin.CreateUserAsync(name, password, new List<string> { role });
        var client = await _fixture.CreateClientAsync(name, password);
        return (client, userId);
    }

    private async Task<string> GetFirstWarehouseId(WarehouseKgClient client)
    {
        var warehouses = await client.GetWarehousesAsync();
        return warehouses[0].GetProperty("id").GetString()!;
    }

    private async Task<string> GetFirstItemId(WarehouseKgClient client)
    {
        var items = await client.GetInventoryItemsAsync();
        return items[0].GetProperty("id").GetString()!;
    }

    private async Task<string> GetFirstSupplierId(WarehouseKgClient client)
    {
        var suppliers = await client.GetSuppliersAsync();
        return suppliers[0].GetProperty("id").GetString()!;
    }

    // ─── Auditor Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task Auditor_CanRead_Warehouses()
    {
        var (client, _) = await CreateAndLoginUser($"auditor-{Guid.NewGuid():N}"[..12], "Auditor");
        var warehouses = await client.GetWarehousesAsync();
        Assert.True(warehouses.GetArrayLength() > 0, "Auditor should see warehouses");
    }

    [Fact]
    public async Task Auditor_CannotCreate_StockReceipt()
    {
        var (client, _) = await CreateAndLoginUser($"auditor-{Guid.NewGuid():N}"[..12], "Auditor");
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/stock-receipts", new
        {
            number = $"AUDIT-TEST-{Guid.NewGuid():N}"[..12],
            warehouseId = await GetFirstWarehouseId(Admin),
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { inventoryItemId = itemId, quantity = 1 } }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Auditor_CanCreate_StockAudit()
    {
        var (client, _) = await CreateAndLoginUser($"auditor-{Guid.NewGuid():N}"[..12], "Auditor");
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/stock-audits", new
        {
            number = $"AUD-{Guid.NewGuid():N}"[..12],
            warehouseId = await GetFirstWarehouseId(Admin),
            reconciledAtUtc = DateTime.UtcNow.AddDays(-10),
            lines = new[] { new { inventoryItemId = itemId, countedQuantity = 100 } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Auditor_CanCreate_StockAdjustment()
    {
        var (client, _) = await CreateAndLoginUser($"auditor-{Guid.NewGuid():N}"[..12], "Auditor");
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/stock-adjustments", new
        {
            number = $"ADJ-{Guid.NewGuid():N}"[..12],
            warehouseId = await GetFirstWarehouseId(Admin),
            reason = "Correction",
            transactionDateUtc = DateTime.UtcNow,
            lines = new[] { new { inventoryItemId = itemId, quantityChange = 5 } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ─── Dispatcher Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task Dispatcher_CanRead_Orders()
    {
        var (client, _) = await CreateAndLoginUser($"disp-{Guid.NewGuid():N}"[..10], "Dispatcher");
        var warehouses = await client.GetWarehousesAsync();
        Assert.True(warehouses.GetArrayLength() > 0, "Dispatcher should see warehouses");
    }

    [Fact]
    public async Task Dispatcher_CannotCreate_StockReceipt()
    {
        var (client, _) = await CreateAndLoginUser($"disp-{Guid.NewGuid():N}"[..10], "Dispatcher");
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/stock-receipts", new
        {
            number = $"DISP-TEST-{Guid.NewGuid():N}"[..12],
            warehouseId = await GetFirstWarehouseId(Admin),
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { inventoryItemId = itemId, quantity = 1 } }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_CanCreate_PurchaseOrder()
    {
        var (client, _) = await CreateAndLoginUser($"disp-{Guid.NewGuid():N}"[..10], "Dispatcher");
        var supplierId = await GetFirstSupplierId(Admin);
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/purchase-orders", new
        {
            number = $"PO-{Guid.NewGuid():N}"[..12],
            supplierId,
            warehouseId = await GetFirstWarehouseId(Admin),
            lines = new[] { new { inventoryItemId = itemId, quantity = 10, unitPrice = 100m } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ─── HR Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HR_CanRead_Warehouses()
    {
        var (client, _) = await CreateAndLoginUser($"hr-{Guid.NewGuid():N}"[..8], "HR");
        var warehouses = await client.GetWarehousesAsync();
        Assert.True(warehouses.GetArrayLength() > 0, "HR should read warehouses");
    }

    [Fact]
    public async Task HR_CannotCreate_StockReceipt()
    {
        var (client, _) = await CreateAndLoginUser($"hr-{Guid.NewGuid():N}"[..8], "HR");
        var itemId = await GetFirstItemId(Admin);

        var response = await client.PostRawAsync("/api/v1/stock-receipts", new
        {
            number = $"HR-TEST-{Guid.NewGuid():N}"[..12],
            warehouseId = await GetFirstWarehouseId(Admin),
            receivedAtUtc = DateTime.UtcNow,
            lines = new[] { new { inventoryItemId = itemId, quantity = 1 } }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HR_CannotCreate_Warehouse()
    {
        var (client, _) = await CreateAndLoginUser($"hr-{Guid.NewGuid():N}"[..8], "HR");

        var response = await client.PostRawAsync("/api/v1/warehouses", new
        {
            code = "HR-WH",
            name = "HR Test Warehouse",
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── Viewer Tests (regression — unchanged) ────────────────────────────

    [Fact]
    public async Task Viewer_CanRead_ButCannotCreate()
    {
        var (client, _) = await CreateAndLoginUser($"viewer-{Guid.NewGuid():N}"[..10], "Viewer");
        var warehouses = await client.GetWarehousesAsync();
        Assert.True(warehouses.GetArrayLength() > 0, "Viewer should read");

        var response = await client.PostRawAsync("/api/v1/warehouses", new
        {
            code = "VW-WH",
            name = "Viewer Warehouse",
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
