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

    /// <summary>
    /// Deletes all draft stock audits for the current tenant to avoid
    /// leftover-data collisions between test runs.
    /// </summary>
    private async Task ClearDraftAuditsAsync(string warehouseId)
    {
        var audits = await Client.GetStockAuditsAsync();
        foreach (var a in audits.EnumerateArray())
        {
            var status = a.GetProperty("status").GetString()!;
            var whId = a.GetProperty("warehouseId").GetString()!;
            if (status == "Draft" && whId == warehouseId)
            {
                var id = a.GetProperty("id").GetString()!;
                try { await Client.DeleteStockAuditAsync(id); } catch { /* best effort */ }
            }
        }
    }

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
            sku = $"CONS-ITEM-{Guid.NewGuid():N}",
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
        await ClearDraftAuditsAsync(whId);
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

    // ═══════════════════════════════════════════════════════════════════════
    // Duplicate audit guard tests
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creating a second draft audit for the same warehouse on the same
    /// day must be rejected to prevent stock drift from sequential completion.
    /// </summary>
    [Fact]
    public async Task CreateAudit_DuplicateSameWarehouseSameDay_IsRejected()
    {
        var (whId, itemId, _, _) = await GetSeedIdsAsync();
        await ClearDraftAuditsAsync(whId);
        var now = DateTime.UtcNow.AddDays(-1); // use yesterday to avoid collisions
        string? id1 = null;

        try
        {
            id1 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-1-{Guid.NewGuid():N}"[..16],
                warehouseId = whId,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                Client.CreateStockAuditAsync(new
                {
                    number = $"DUP-2-{Guid.NewGuid():N}"[..16],
                    warehouseId = whId,
                    reconciledAtUtc = now,
                    lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
                }));

            Assert.Contains("черновой аудит", ex.Message);
        }
        finally
        {
            if (id1 != null)
            {
                try { await Client.CancelStockAuditAsync(id1.Trim('"')); } catch { /* best effort */ }
            }
        }
    }

    /// <summary>
    /// A cancelled draft audit should not block creation of a new audit
    /// for the same warehouse+day.
    /// </summary>
    [Fact]
    public async Task CreateAudit_AfterCancellingDraft_Succeeds()
    {
        var (whId, itemId, _, _) = await GetSeedIdsAsync();
        await ClearDraftAuditsAsync(whId);
        var now = DateTime.UtcNow.AddDays(-2);
        string? id2 = null;

        try
        {
            var id1 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-C-{Guid.NewGuid():N}"[..16],
                warehouseId = whId,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            await Client.CancelStockAuditAsync(id1.Trim('"'));

            id2 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-C2-{Guid.NewGuid():N}"[..16],
                warehouseId = whId,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            Assert.NotEmpty(id2);
        }
        finally
        {
            if (id2 != null)
            {
                try { await Client.CancelStockAuditAsync(id2.Trim('"')); } catch { /* best effort */ }
            }
        }
    }

    /// <summary>
    /// A completed audit should not block creation of a new audit
    /// for the same warehouse+day.
    /// </summary>
    [Fact]
    public async Task CreateAudit_AfterCompletingDraft_Succeeds()
    {
        var (whId, itemId, _, _) = await GetSeedIdsAsync();
        await ClearDraftAuditsAsync(whId);
        var now = DateTime.UtcNow.AddDays(-3);
        string? id2 = null;

        try
        {
            var id1 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-COMP-{Guid.NewGuid():N}"[..16],
                warehouseId = whId,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            // Client is already admin from SharedFixture
            await Client.CompleteStockAuditAsync(id1.Trim('"'));

            id2 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-COMP2-{Guid.NewGuid():N}"[..16],
                warehouseId = whId,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            Assert.NotEmpty(id2);
        }
        finally
        {
            if (id2 != null)
            {
                try { await Client.CancelStockAuditAsync(id2.Trim('"')); } catch { /* best effort */ }
            }
        }
    }

    /// <summary>
    /// Audits for different warehouses on the same day are independent.
    /// </summary>
    [Fact]
    public async Task CreateAudit_DifferentWarehouses_Succeeds()
    {
        var (whId1, itemId, _, _) = await GetSeedIdsAsync();
        await ClearDraftAuditsAsync(whId1);

        var warehouses = await Client.GetWarehousesAsync();
        string? whId2 = null;
        foreach (var w in warehouses.EnumerateArray())
        {
            var id = w.GetProperty("id").GetString()!;
            if (id != whId1) { whId2 = id; break; }
        }

        if (whId2 is null) return;

        await ClearDraftAuditsAsync(whId2);

        var now = DateTime.UtcNow.AddDays(-4);
        string? id1 = null, id2 = null;

        try
        {
            id1 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-W1-{Guid.NewGuid():N}"[..16],
                warehouseId = whId1,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            id2 = await Client.CreateStockAuditAsync(new
            {
                number = $"DUP-W2-{Guid.NewGuid():N}"[..16],
                warehouseId = whId2,
                reconciledAtUtc = now,
                lines = new[] { new { inventoryItemId = itemId, countedQuantity = 10m } }
            });

            Assert.NotEmpty(id1);
            Assert.NotEmpty(id2);
        }
        finally
        {
            if (id1 != null) { try { await Client.CancelStockAuditAsync(id1.Trim('"')); } catch { } }
            if (id2 != null) { try { await Client.CancelStockAuditAsync(id2.Trim('"')); } catch { } }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Dispatching — auto-ship inventory deduction tests
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// When a delivery stop is completed via the dispatching workflow,
    /// the auto-ship logic must deduct inventory and the movement history
    /// must show a "Продажа" row for the sales order.
    /// </summary>
    [Fact]
    public async Task DispatchingAutoShip_DeductsInventory_AndShowsInMovementHistory()
    {
        var (whId, itemId, _, customerId) = await GetSeedIdsAsync();
        var warehouseId = ToGuid(whId);
        var invItemId = ToGuid(itemId);
        var now = DateTime.UtcNow;

        // 1. Seed stock via receipt (+20)
        var recId = await Client.CreateReceiptAsync(new
        {
            number = $"DISP-REC-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 20m } }
        });
        await Client.CompleteReceiptAsync(recId.Trim('"'));

        // 2. Create a confirmed sales order
        var soId = await Client.CreateSalesOrderAsync(new
        {
            number = $"DISP-SO-{Guid.NewGuid():N}"[..16],
            customerId,
            warehouseId = whId,
            expectedDateUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 7m, unitPrice = 100m } }
        });
        await Client.ConfirmSalesOrderAsync(soId.Trim('"'));

        // 3. Create a delivery route (Planned)
        var routeId = (await Client.CreateRouteAsync(new
        {
            code = $"DISP-R-{Guid.NewGuid():N}"[..8],
            date = now
        })).Trim('"');

        // 4. Create a stop on the route
        var stopId = (await Client.CreateStopAsync(routeId, new
        {
            routeId,
            sequenceNumber = 1,
            customerId,
            address = "Test Address 1",
            latitude = 42.87,
            longitude = 74.59
        })).Trim('"');

        // 5. Assign the sales order to the stop as a shipment
        await Client.AssignShipmentAsync(stopId, ToGuid(soId));

        // 6. Start the route (Planned → InProgress)
        await Client.StartRouteAsync(routeId);

        // 7. Arrive at the stop (Pending → InProgress)
        await Client.ArriveAtStopAsync(routeId, stopId);

        // 8. Complete the stop (InProgress → Completed) — triggers auto-ship
        await Client.CompleteStopAsync(routeId, stopId);

        // 9. Complete the route
        await Client.CompleteRouteAsync(routeId);

        // ═══════════════════════════════════════════════════════════════
        // VERIFY inventory deducted
        // ═══════════════════════════════════════════════════════════════
        var item = await Client.GetInventoryItemAsync(invItemId);
        var qtyOnHand = GetDecimal(item, "quantityOnHand");
        Assert.Equal(13m, qtyOnHand); // 20 - 7 = 13

        // ═══════════════════════════════════════════════════════════════
        // VERIFY sales order is Shipped
        // ═══════════════════════════════════════════════════════════════
        var soDetail = await Client.GetSalesOrderAsync(ToGuid(soId));
        var soStatus = soDetail.GetProperty("status").GetString()!;
        Assert.Equal("Shipped", soStatus);

        // ═══════════════════════════════════════════════════════════════
        // VERIFY movement history shows "Продажа" row
        // ═══════════════════════════════════════════════════════════════
        var movements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var movArray = movements.EnumerateArray().ToList();

        var saleRows = movArray
            .Where(m => m.GetProperty("operationType").GetString() == "Продажа")
            .ToList();
        Assert.NotEmpty(saleRows);
        Assert.Contains(saleRows, m =>
            m.GetProperty("documentNumber").GetString()!.StartsWith("DISP-SO-")
            && GetDecimal(m, "quantityChange") == -7m);

        // ═══════════════════════════════════════════════════════════════
        // VERIFY warehouse stock report consistency
        // ═══════════════════════════════════════════════════════════════
        var finalBalance = GetDecimal(movArray[^1], "runningBalance");

        var nonAuditSum = 0m;
        foreach (var m in movArray)
        {
            var opType = m.GetProperty("operationType").GetString()!;
            if (!opType.StartsWith("Аудит"))
                nonAuditSum += GetDecimal(m, "quantityChange");
        }

        var stockAll = await Client.GetWarehouseStockAsync(warehouseId);
        var ourItem = stockAll.EnumerateArray()
            .FirstOrDefault(i => i.GetProperty("inventoryItemId").GetString()!
                .Equals(invItemId.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(default(JsonElement), ourItem);

        var netChange = GetDecimal(ourItem, "netChange");
        Assert.Equal(nonAuditSum, netChange);
        Assert.Equal(finalBalance, GetDecimal(ourItem, "quantityOnHand"));
    }

    /// <summary>
    /// Sales orders without a WarehouseId should NOT appear in movement history.
    /// This guards against the bug where auto-shipped orders with NULL WarehouseId
    /// were invisible in inventory movement tracking.
    /// </summary>
    [Fact]
    public async Task DispatchingAutoShip_MissingWarehouseId_NotInMovementHistory()
    {
        var (whId, itemId, _, customerId) = await GetSeedIdsAsync();
        var warehouseId = ToGuid(whId);
        var invItemId = ToGuid(itemId);
        var now = DateTime.UtcNow;

        // Seed stock
        var recId = await Client.CreateReceiptAsync(new
        {
            number = $"DISP2-REC-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 10m } }
        });
        await Client.CompleteReceiptAsync(recId.Trim('"'));

        // Create a sales order WITHOUT WarehouseId (simulating the bug)
        var soId = await Client.CreateSalesOrderAsync(new
        {
            number = $"DISP2-SO-{Guid.NewGuid():N}"[..16],
            customerId,
            // warehouseId intentionally omitted
            expectedDateUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 3m, unitPrice = 50m } }
        });
        await Client.ConfirmSalesOrderAsync(soId.Trim('"'));

        // Ship via direct API (bypassing dispatching)
        await Client.ShipSalesOrderAsync(soId.Trim('"'));

        // Verify the sales order IS Shipped
        var soDetail = await Client.GetSalesOrderAsync(ToGuid(soId));
        Assert.Equal("Shipped", soDetail.GetProperty("status").GetString());

        // But movement history for the specific warehouse should NOT show it
        var movements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var movArray = movements.EnumerateArray().ToList();

        var disp2Rows = movArray
            .Where(m => m.GetProperty("documentNumber").GetString()!.StartsWith("DISP2-SO-"))
            .ToList();
        Assert.Empty(disp2Rows); // NULL WarehouseId → invisible in any warehouse filter
    }

    /// <summary>
    /// After the WarehouseId fix, a sales order WITH WarehouseId shipped via
    /// the manual Ship endpoint must appear in movement history for that warehouse.
    /// </summary>
    [Fact]
    public async Task ManualShip_WithWarehouseId_ShowsInMovementHistory()
    {
        var (whId, itemId, _, customerId) = await GetSeedIdsAsync();
        var warehouseId = ToGuid(whId);
        var invItemId = ToGuid(itemId);
        var now = DateTime.UtcNow;

        // Seed stock
        var recId = await Client.CreateReceiptAsync(new
        {
            number = $"DISP3-REC-{Guid.NewGuid():N}"[..16],
            warehouseId = whId,
            receivedAtUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 15m } }
        });
        await Client.CompleteReceiptAsync(recId.Trim('"'));

        // Create a sales order WITH WarehouseId
        var soId = await Client.CreateSalesOrderAsync(new
        {
            number = $"DISP3-SO-{Guid.NewGuid():N}"[..16],
            customerId,
            warehouseId = whId,
            expectedDateUtc = now,
            lines = new[] { new { inventoryItemId = itemId, quantity = 4m, unitPrice = 75m } }
        });
        await Client.ConfirmSalesOrderAsync(soId.Trim('"'));

        // Ship via direct API
        await Client.ShipSalesOrderAsync(soId.Trim('"'));

        // Verify the sales order IS Shipped
        var soDetail = await Client.GetSalesOrderAsync(ToGuid(soId));
        Assert.Equal("Shipped", soDetail.GetProperty("status").GetString());

        // Movement history MUST show the "Продажа" row
        var movements = await Client.GetItemMovementsAsync(invItemId, warehouseId);
        var movArray = movements.EnumerateArray().ToList();

        var disp3Rows = movArray
            .Where(m => m.GetProperty("documentNumber").GetString()!.StartsWith("DISP3-SO-")
                        && m.GetProperty("operationType").GetString() == "Продажа")
            .ToList();
        Assert.Single(disp3Rows);
        Assert.Equal(-4m, GetDecimal(disp3Rows[0], "quantityChange"));

        // Verify running balance is correct: 15 - 4 = 11
        Assert.Equal(11m, GetDecimal(movArray[^1], "runningBalance"));
    }
}
