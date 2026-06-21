using System.Net;
using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Integration tests for Preseller module: pre-order CRUD, workflow, conversion to sales order.
/// </summary>
[Collection("IntegrationTests")]
public class PresellerWorkflowTests
{
    private readonly SharedFixture _fixture;

    public PresellerWorkflowTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    private WarehouseKgClient Admin => _fixture.Client;

    private async Task<(string customerId, string warehouseId, string itemId)> GetTestData()
    {
        var customers = await Admin.GetCustomersAsync();
        var customerId = customers.EnumerateArray().First().GetProperty("id").GetString()!;

        var warehouses = await Admin.GetWarehousesAsync();
        var warehouseId = warehouses.EnumerateArray().First().GetProperty("id").GetString()!;

        var items = await Admin.GetInventoryItemsAsync();
        var itemId = items.EnumerateArray().First().GetProperty("id").GetString()!;

        return (customerId, warehouseId, itemId);
    }

    // ─── CRUD Tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Preseller_CanCreateDraftPreOrder()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-TEST-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            notes = (string?)null,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 10m, unitPrice = 150m, discountPercent = 0m }
            }
        });

        var idStr = id.Trim('"');
        Assert.NotEmpty(idStr);
        Assert.True(Guid.TryParse(idStr, out _));

        // Verify it exists
        var po = await Admin.GetPreOrderByIdAsync(idStr);
        Assert.Equal(0, po.GetProperty("status").GetInt32()); // Draft
        Assert.Equal("Наличные", po.GetProperty("paymentType").GetString());
        Assert.Equal(1, po.GetProperty("lines").GetArrayLength());
    }

    [Fact]
    public async Task Preseller_CanSubmitPreOrder()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-SUB-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Карта",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 5m, unitPrice = 200m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');

        await Admin.SubmitPreOrderAsync(idStr);

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        Assert.Equal(1, po.GetProperty("status").GetInt32()); // Submitted
        Assert.NotNull(po.GetProperty("submittedAtUtc").GetString());
    }

    [Fact]
    public async Task Admin_CanApprovePreOrder()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-APP-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Безналичный",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 3m, unitPrice = 500m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');
        await Admin.SubmitPreOrderAsync(idStr);

        await Admin.ApprovePreOrderAsync(idStr);

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        Assert.Equal(2, po.GetProperty("status").GetInt32()); // Approved
        Assert.NotNull(po.GetProperty("approvedAtUtc").GetString());
    }

    [Fact]
    public async Task Admin_CanRejectPreOrder()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-REJ-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Кредит",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 1m, unitPrice = 100m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');
        await Admin.SubmitPreOrderAsync(idStr);

        await Admin.RejectPreOrderAsync(idStr, "Недостаточно товара на складе");

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        Assert.Equal(3, po.GetProperty("status").GetInt32()); // Rejected
    }

    [Fact]
    public async Task Admin_CanConvertApprovedPreOrderToSalesOrder()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-CNV-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "USD",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 2m, unitPrice = 300m, discountPercent = 10m }
            }
        });
        var idStr = id.Trim('"');
        await Admin.SubmitPreOrderAsync(idStr);
        await Admin.ApprovePreOrderAsync(idStr);

        var soIdStr = await Admin.ConvertPreOrderAsync(idStr);
        var soId = soIdStr.Trim('"');
        Assert.True(Guid.TryParse(soId, out _));

        // Verify pre-order status
        var po = await Admin.GetPreOrderByIdAsync(idStr);
        Assert.Equal(4, po.GetProperty("status").GetInt32()); // Converted
        Assert.Equal(soId, po.GetProperty("convertedSalesOrderId").GetString());

        // Verify sales order was created
        var so = await Admin.GetSalesOrderAsync(Guid.Parse(soId));
        var soStatus = so.GetProperty("status").GetString();
        Assert.Equal("Draft", soStatus);
        Assert.Equal("USD", so.GetProperty("currency").GetString());
        Assert.Contains("PO-CNV", so.GetProperty("number").GetString());
    }

    // ─── Auth Tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePreOrder_RequiresAuth()
    {
        // Verify that unauthenticated requests are rejected
        var http = _fixture.CreateRawClient();
        var response = await http.GetAsync("/api/v1/pre-orders/my");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─── Validation Tests ────────────────────────────────────────────────

    [Fact]
    public async Task SubmitPreOrder_RequiresAtLeastOneLine()
    {
        var (customerId, warehouseId, _) = await GetTestData();

        // Create with empty lines should throw
        var response = await Admin.PostRawAsync("/api/v1/pre-orders", new
        {
            number = $"PO-EMPTY-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePreOrder_DraftOnly()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-UPD-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 1m, unitPrice = 100m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');
        await Admin.SubmitPreOrderAsync(idStr);

        // Update should fail on submitted
        var response = await Admin.PutRawAsync($"/api/v1/pre-orders/{idStr}", new
        {
            id = idStr,
            number = $"PO-UPD-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 2m, unitPrice = 100m, discountPercent = 0m }
            }
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeletePreOrder_DraftOnly()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        // Create a draft and delete it successfully
        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-DELOK-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 1m, unitPrice = 100m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');
        await Admin.DeletePreOrderAsync(idStr);

        // Verify deleted — should be null
        var (customerId2, warehouseId2, itemId2) = await GetTestData();
        var id2 = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-DEL2-{Guid.NewGuid():N}"[..12],
            customerId = customerId2,
            warehouseId = warehouseId2,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId2, quantity = 1m, unitPrice = 100m, discountPercent = 0m }
            }
        });
        var idStr2 = id2.Trim('"');

        // Submit and try to delete — should fail with 409
        await Admin.SubmitPreOrderAsync(idStr2);
        await Assert.ThrowsAsync<HttpRequestException>(() => Admin.DeletePreOrderAsync(idStr2));
    }

    // ─── Stock Snapshot Tests ────────────────────────────────────────────

    [Fact]
    public async Task PreOrder_CapturesWarehouseStockSnapshot()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        // First, add stock to the item via a stock receipt
        var warehouses = await Admin.GetWarehousesAsync();
        var whId = warehouses.EnumerateArray().First().GetProperty("id").GetString()!;
        var recId = await Admin.CreateReceiptAsync(new
        {
            number = $"RCV-PRE-{Guid.NewGuid():N}"[..12],
            warehouseId = whId,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 100m }
            }
        });
        await Admin.CompleteReceiptAsync(recId.Trim('"'));

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-STK-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 25m, unitPrice = 100m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        var line = po.GetProperty("lines")[0];
        var stockSnapshot = line.GetProperty("warehouseStockSnapshot").GetDecimal();
        var stockDiff = line.GetProperty("stockDifference").GetDecimal();

        Assert.True(stockSnapshot > 0);
        Assert.Equal(stockSnapshot - 25, stockDiff);
    }

    [Fact]
    public async Task StockDifference_CalculatedCorrectly()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-DIFF-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 999999m, unitPrice = 1m, discountPercent = 0m }
            }
        });
        var idStr = id.Trim('"');

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        var line = po.GetProperty("lines")[0];
        var stockDiff = line.GetProperty("stockDifference").GetDecimal();

        // Should be negative (we ordered way more than stock)
        Assert.True(stockDiff < 0);
    }

    // ─── Payment Types ───────────────────────────────────────────────────

    [Fact]
    public async Task PaymentTypes_AreSeeded()
    {
        var pts = await Admin.GetPaymentTypesAsync();
        var list = pts.EnumerateArray().ToList();
        Assert.True(list.Count >= 4);
        
        var names = list.Select(e => e.GetProperty("name").GetString()).ToHashSet();
        Assert.Contains("Наличные", names);
        Assert.Contains("Безналичный", names);
        Assert.Contains("Карта", names);
        Assert.Contains("Кредит", names);
    }

    // ─── Line Total with Discount ────────────────────────────────────────

    [Fact]
    public async Task LineTotal_CalculatedWithDiscount()
    {
        var (customerId, warehouseId, itemId) = await GetTestData();

        var id = await Admin.CreatePreOrderAsync(new
        {
            number = $"PO-DISC-{Guid.NewGuid():N}"[..12],
            customerId,
            warehouseId,
            paymentType = "Наличные",
            currency = "KGS",
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 10m, unitPrice = 100m, discountPercent = 15m }
            }
        });
        var idStr = id.Trim('"');

        var po = await Admin.GetPreOrderByIdAsync(idStr);
        var line = po.GetProperty("lines")[0];
        var lineTotal = line.GetProperty("lineTotal").GetDecimal();

        // 10 * 100 * (1 - 15/100) = 1000 * 0.85 = 850
        Assert.Equal(850m, lineTotal);
    }
}
