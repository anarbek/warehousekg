using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Ensures inventory item movement history is always in sync with
/// warehouse stock reports — the running balance from movements must
/// match net change from the warehouse report for any date range.
/// </summary>
[Collection("IntegrationTests")]
public class WarehouseStockConsistencyTests
{
    private readonly SharedFixture _fixture;

    public WarehouseStockConsistencyTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    private WarehouseKgClient Client => _fixture.Client;

    private async Task<(string warehouseId, string itemId, string supplierId)> GetSeedIdsAsync()
    {
        var warehouses = await Client.GetWarehousesAsync();
        var warehouseId = warehouses[0].GetProperty("id").GetString()!;

        var items = await Client.GetInventoryItemsAsync();
        var itemId = items[0].GetProperty("id").GetString()!;

        var suppliers = await Client.GetSuppliersAsync();
        var supplierId = suppliers[0].GetProperty("id").GetString()!;

        return (warehouseId, itemId, supplierId);
    }

    private static Guid ToGuid(string id) => Guid.Parse(id.Trim('"'));

    private static decimal GetDecimal(JsonElement el, string prop) =>
        el.GetProperty(prop).GetDecimal();

    /// <summary>
    /// Full workflow: creates receipts, pick, purchase order, and transfer,
    /// then verifies movement history balances match warehouse stock report.
    /// </summary>
    [Fact]
    public async Task FullWorkflow_MovementHistoryMatchesWarehouseStock()
    {
        var (whId, itemId, supplierId) = await GetSeedIdsAsync();
        var warehouseId = ToGuid(whId);
        var invItemId = ToGuid(itemId);

        // ── 1. Receipt (+10) ──────────────────────────────────
        var now = DateTime.UtcNow;
        var r1Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R1-{Guid.NewGuid():N}"[..12],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 10m } }
        });
        await Client.CompleteReceiptAsync(r1Id.Trim('"'));

        // ── 2. Pick (-3) ──────────────────────────────────────
        var pickId = await Client.CreatePickOrderAsync(new
        {
            number = $"CONS-P-{Guid.NewGuid():N}"[..10],
            warehouseId = whId,
            plannedPickDate = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 3m } }
        });
        await Client.CompletePickOrderAsync(pickId.Trim('"'));

        // ── 3. Receipt (+5) ───────────────────────────────────
        var r2Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R2-{Guid.NewGuid():N}"[..12],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 5m } }
        });
        await Client.CompleteReceiptAsync(r2Id.Trim('"'));

        // ── 4. Purchase order received (+4) ──────────────────
        var poId = await Client.CreatePurchaseOrderAsync(new
        {
            number = $"CONS-PO-{Guid.NewGuid():N}"[..10],
            supplierId,
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 4m, unitPrice = 0m } }
        });
        await Client.SubmitPurchaseOrderAsync(poId.Trim('"'));
        await Client.ReceivePurchaseOrderAsync(poId.Trim('"'));

        // ── 5. Transfer OUT (-2) ──────────────────────────────
        var warehouses = await Client.GetWarehousesAsync();
        string? destWhId = null;
        foreach (var w in warehouses.EnumerateArray())
        {
            var id = w.GetProperty("id").GetString()!;
            if (id != whId) { destWhId = id; break; }
        }

        string? trId = null;
        if (destWhId != null)
        {
            trId = await Client.CreateStockTransferAsync(new
            {
                number = $"CONS-TR-{Guid.NewGuid():N}"[..10],
                sourceWarehouseId = whId,
                destinationWarehouseId = destWhId,
                transferredAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, quantity = 2m } }
            });
            await Client.CompleteStockTransferAsync(trId.Trim('"'));
        }

        // ── 6. Receipt (+6) ───────────────────────────────────
        var r3Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R3-{Guid.NewGuid():N}"[..12],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 6m } }
        });
        await Client.CompleteReceiptAsync(r3Id.Trim('"'));

        // ═══════════════════════════════════════════════════════════════
        // VERIFY: Movement history final balance = Warehouse stock net change
        // ═══════════════════════════════════════════════════════════════
        var movements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var movArray = movements.EnumerateArray().ToList();

        // Get running balance from last movement
        var finalBalance = movArray.Count > 0
            ? GetDecimal(movArray[^1], "runningBalance")
            : 0m;

        var stockAll = await Client.GetWarehouseStockAsync(warehouseId);
        var ourItem = stockAll.EnumerateArray()
            .FirstOrDefault(i => i.GetProperty("inventoryItemId").GetString()!
                .Equals(invItemId.ToString(), StringComparison.OrdinalIgnoreCase));

        Assert.NotEqual(default(JsonElement), ourItem);
        var netChange = GetDecimal(ourItem, "netChange");

        // Core invariant: movement history final balance == warehouse stock net change
        Assert.Equal(finalBalance, netChange);

        // Also verify our operations are present in movement history
        var expectedCount = trId != null ? 6 : 5;
        var consMovs = movArray
            .Where(m => m.GetProperty("documentNumber").GetString()!.StartsWith("CONS-"))
            .ToList();
        Assert.True(consMovs.Count >= expectedCount,
            $"Expected at least {expectedCount} CONS- movements, got {consMovs.Count}");
    }
}
