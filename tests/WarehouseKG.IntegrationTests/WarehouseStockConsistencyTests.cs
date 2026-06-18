using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Ensures inventory item movement history is always in sync with
/// warehouse stock reports — the running balance from movements must
/// match the warehouse stock Всего, and the sum of non-audit deltas
/// must match Оборот.
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

    private async Task<(string warehouseId, string itemId, string supplierId, string customerId)> GetSeedIdsAsync()
    {
        var warehouses = await Client.GetWarehousesAsync();
        var warehouseId = warehouses[0].GetProperty("id").GetString()!;

        var suppliers = await Client.GetSuppliersAsync();
        var supplierId = suppliers[0].GetProperty("id").GetString()!;

        var customers = await Client.GetCustomersAsync();
        string customerId;
        if (customers.GetArrayLength() == 0)
        {
            customerId = (await Client.CreateCustomerAsync(new { code = "TEST", name = "Test Customer" })).Trim('"');
        }
        else
        {
            customerId = customers[0].GetProperty("id").GetString()!;
        }

        // Create a fresh item to avoid accumulated test data
        var categories = await Client.GetCategoriesAsync();
        var categoryId = categories[0].GetProperty("id").GetString()!;
        var uoms = await Client.GetUnitsOfMeasureAsync();
        var uomId = uoms[0].GetProperty("id").GetString()!;
        var itemId = (await Client.CreateInventoryItemAsync(new
        {
            sku = $"CONS-ITEM-{Guid.NewGuid():N}"[..12],
            name = "Consistency Test Item",
            categoryId,
            unitOfMeasureId = uomId,
            reorderLevel = 0m
        })).Trim('"');

        return (warehouseId, itemId, supplierId, customerId);
    }

    private static Guid ToGuid(string id) => Guid.Parse(id.Trim('"'));

    private static decimal GetDecimal(JsonElement el, string prop) =>
        el.GetProperty(prop).GetDecimal();

    /// <summary>
    /// Full workflow: receipts, pick, purchase order, transfer, sales order,
    /// stock adjustment, and stock audit.  Verifies both Оборот (netChange,
    /// excluding audit deltas) and Всего (quantityOnHand, including everything)
    /// match the movement history.
    /// </summary>
    [Fact]
    public async Task FullWorkflow_MovementHistoryMatchesWarehouseStock()
    {
        var (whId, itemId, supplierId, customerId) = await GetSeedIdsAsync();
        var warehouseId = ToGuid(whId);
        var invItemId = ToGuid(itemId);

        var now = DateTime.UtcNow;

        // ── 1. Receipt (+10) ──────────────────────────────────
        var r1Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R1-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 10m } }
        });
        await Client.CompleteReceiptAsync(r1Id.Trim('"'));

        // ── 2. Pick (-3) ──────────────────────────────────────
        var pickId = await Client.CreatePickOrderAsync(new
        {
            number = $"CONS-P-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            plannedPickDate = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 3m } }
        });
        await Client.CompletePickOrderAsync(pickId.Trim('"'));

        // ── 3. Receipt (+5) ───────────────────────────────────
        var r2Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R2-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 5m } }
        });
        await Client.CompleteReceiptAsync(r2Id.Trim('"'));

        // ── 4. Purchase order received (+4) ──────────────────
        var poId = await Client.CreatePurchaseOrderAsync(new
        {
            number = $"CONS-PO-{Guid.NewGuid():N}"[..16],
            supplierId,
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 4m, unitPrice = 0m } }
        });
        await Client.SubmitPurchaseOrderAsync(poId.Trim('"'));
        await Client.ReceivePurchaseOrderAsync(poId.Trim('"'));

        // ── 5. Stock Adjustment (+2) ─────────────────────────
        var adjId = await Client.CreateStockAdjustmentAsync(new
        {
            number = $"CONS-ADJ-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            adjustedAtUtc = now,
            reason = "Correction",
            lines = new[] { new { inventoryItemId = itemId, quantityChange = 2m } }
        });
        await Client.CompleteStockAdjustmentAsync(adjId.Trim('"'));

        // ── 6. Transfer OUT (-2) ──────────────────────────────
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
                number = $"CONS-TR-{Guid.NewGuid():N}"[..16],
                sourceWarehouseId = whId,
                destinationWarehouseId = destWhId,
                transferredAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, quantity = 2m } }
            });
            await Client.CompleteStockTransferAsync(trId.Trim('"'));
        }

        // ── 7. Sales Order shipped (-5) ──────────────────────
        var soId = await Client.CreateSalesOrderAsync(new
        {
            number = $"CONS-SO-{Guid.NewGuid():N}"[..16],
            customerId,
            warehouseId = whId,
            expectedDateUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 5m, unitPrice = 0m } }
        });
        await Client.ConfirmSalesOrderAsync(soId.Trim('"'));
        await Client.ShipSalesOrderAsync(soId.Trim('"'));

        // ── 8. Receipt (+6) ───────────────────────────────────
        var r3Id = await Client.CreateReceiptAsync(new
        {
            number = $"CONS-R3-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 6m } }
        });
        await Client.CompleteReceiptAsync(r3Id.Trim('"'));

        // ── 9. Stock Audit — count +4 above system ──────────
        // Capture current total from movement history to compute audit target
        var preMovements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var preMovArray = preMovements.EnumerateArray().ToList();
        var preBalance = preMovArray.Count > 0
            ? GetDecimal(preMovArray[^1], "runningBalance")
            : 0m;
        var auditCounted = preBalance + 4m;

        var auditId = await Client.CreateStockAuditAsync(new
        {
            number = $"CONS-AUD-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            reconciledAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, countedQuantity = auditCounted } }
        });
        await Client.CompleteStockAuditAsync(auditId.Trim('"'));

        // ═══════════════════════════════════════════════════════════════
        // VERIFY against movement history and warehouse stock report
        // ═══════════════════════════════════════════════════════════════
        var movements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var movArray = movements.EnumerateArray().ToList();
        Assert.NotEmpty(movArray);

        // Final running balance from movement history (includes audit)
        var finalBalance = GetDecimal(movArray[^1], "runningBalance");

        // Sum of non-audit deltas from movement history
        var nonAuditSum = 0m;
        foreach (var m in movArray)
        {
            var opType = m.GetProperty("operationType").GetString()!;
            if (!opType.StartsWith("Аудит"))
                nonAuditSum += GetDecimal(m, "quantityChange");
        }

        // Get warehouse stock report (full range, no date filter)
        var stockAll = await Client.GetWarehouseStockAsync(warehouseId);
        var ourItem = stockAll.EnumerateArray()
            .FirstOrDefault(i => i.GetProperty("inventoryItemId").GetString()!
                .Equals(invItemId.ToString(), StringComparison.OrdinalIgnoreCase));

        Assert.NotEqual(default(JsonElement), ourItem);
        var netChange = GetDecimal(ourItem, "netChange");          // Оборот
        var qtyOnHand = GetDecimal(ourItem, "quantityOnHand");     // Всего

        // Invariant 1: Оборот (netChange) = sum of non-audit movement deltas
        Assert.Equal(nonAuditSum, netChange);

        // Invariant 2: Всего (quantityOnHand) = final running balance (includes audit)
        Assert.Equal(finalBalance, qtyOnHand);

        // Verify our CONS- operations are present in movement history
        var minExpected = trId != null ? 9 : 8;
        var consMovs = movArray
            .Where(m => m.GetProperty("documentNumber").GetString()!.StartsWith("CONS-"))
            .ToList();
        Assert.True(consMovs.Count >= minExpected,
            $"Expected at least {minExpected} CONS- movements, got {consMovs.Count}");
    }
}
