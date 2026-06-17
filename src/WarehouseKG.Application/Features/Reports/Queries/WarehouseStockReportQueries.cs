using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Reports.Queries;

/// <summary>
/// Calculates per-item stock at a warehouse by replaying all completed operations within a date range.
/// </summary>
public record GetWarehouseStockReportQuery(
    Guid WarehouseId,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<IReadOnlyList<WarehouseStockItemDto>>;

public class GetWarehouseStockReportQueryHandler
    : IRequestHandler<GetWarehouseStockReportQuery, IReadOnlyList<WarehouseStockItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWarehouseStockReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WarehouseStockItemDto>> Handle(
        GetWarehouseStockReportQuery request,
        CancellationToken cancellationToken)
    {
        var warehouseId = request.WarehouseId;

        // ─── Build net change per item from all completed operations ──────
        var deltas = new Dictionary<Guid, decimal>();

        // Helper: apply date filter to completed operations
        IQueryable<T> FilterByDate<T>(IQueryable<T> query, DateTime? completedField) where T : class
        {
            if (request.DateFrom.HasValue && completedField.HasValue)
                query = query.Where(_ => EF.Property<DateTime>(_!, "CompletedAt") >= request.DateFrom!.Value);
            // Note: filtering by exact timestamp field depends on entity; we filter in-memory below
            return query;
        }

        // 1. StockReceipts (Completed) → add line quantities
        var receipts = await _context.StockReceipts
            .Where(r => r.WarehouseId == warehouseId && r.Status == StockOperationStatus.Completed)
            .Include(r => r.Lines)
            .ToListAsync(cancellationToken);

        foreach (var r in receipts)
        {
            if (!IsInDateRange(r.ReceivedAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in r.Lines)
                AddDelta(deltas, l.InventoryItemId, l.Quantity);
        }

        // 2. StockAdjustments (Completed) → signed quantity change
        var adjustments = await _context.StockAdjustments
            .Where(a => a.WarehouseId == warehouseId && a.Status == StockOperationStatus.Completed)
            .Include(a => a.Lines)
            .ToListAsync(cancellationToken);

        foreach (var a in adjustments)
        {
            if (!IsInDateRange(a.AdjustedAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in a.Lines)
                AddDelta(deltas, l.InventoryItemId, l.QuantityChange);
        }

        // 3. StockAudits (Completed) → variance (Counted − System)
        var audits = await _context.StockAudits
            .Where(a => a.WarehouseId == warehouseId && a.Status == StockOperationStatus.Completed)
            .Include(a => a.Lines)
            .ToListAsync(cancellationToken);

        foreach (var a in audits)
        {
            if (!IsInDateRange(a.ReconciledAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in a.Lines)
            {
                var variance = l.CountedQuantity - l.SystemQuantity;
                AddDelta(deltas, l.InventoryItemId, variance);
            }
        }

        // 4. StockTransfers TO this warehouse (Completed) → add
        var transfersIn = await _context.StockTransfers
            .Where(t => t.DestinationWarehouseId == warehouseId && t.Status == StockOperationStatus.Completed)
            .Include(t => t.Lines)
            .ToListAsync(cancellationToken);

        foreach (var t in transfersIn)
        {
            if (!IsInDateRange(t.TransferredAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in t.Lines)
                AddDelta(deltas, l.InventoryItemId, l.Quantity);
        }

        // 5. StockTransfers FROM this warehouse (Completed) → subtract
        var transfersOut = await _context.StockTransfers
            .Where(t => t.SourceWarehouseId == warehouseId && t.Status == StockOperationStatus.Completed)
            .Include(t => t.Lines)
            .ToListAsync(cancellationToken);

        foreach (var t in transfersOut)
        {
            if (!IsInDateRange(t.TransferredAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in t.Lines)
                AddDelta(deltas, l.InventoryItemId, -l.Quantity);
        }

        // 6. PickOrders (Completed) → subtract
        var picks = await _context.PickOrders
            .Where(p => p.WarehouseId == warehouseId && p.Status == StockOperationStatus.Completed)
            .Include(p => p.Lines)
            .ToListAsync(cancellationToken);

        foreach (var p in picks)
        {
            if (!IsInDateRange(p.PickedAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in p.Lines)
                AddDelta(deltas, l.InventoryItemId, -l.Quantity);
        }

        // 7. PurchaseOrders (Received) → add
        var pos = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .Where(po => po.WarehouseId == warehouseId && po.Status == PurchaseOrderStatus.Received)
            .ToListAsync(cancellationToken);

        foreach (var po in pos)
        {
            if (!IsInDateRange(po.ReceivedAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in po.Lines)
                AddDelta(deltas, l.InventoryItemId, l.Quantity);
        }

        // 8. SalesOrders (Shipped) → subtract (if tied to this warehouse)
        var sos = await _context.SalesOrders
            .Include(so => so.Lines)
            .Where(so => so.WarehouseId == warehouseId && so.Status == SalesOrderStatus.Shipped)
            .ToListAsync(cancellationToken);

        foreach (var so in sos)
        {
            if (!IsInDateRange(so.ShippedAtUtc, request.DateFrom, request.DateTo)) continue;
            foreach (var l in so.Lines)
                AddDelta(deltas, l.InventoryItemId, -l.Quantity);
        }

        // ─── Enrich with item details ─────────────────────────────────────
        var itemIds = deltas.Keys.ToList();
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var result = new List<WarehouseStockItemDto>();
        foreach (var (itemId, netChange) in deltas.OrderByDescending(kv => kv.Value))
        {
            if (!items.TryGetValue(itemId, out var item)) continue;

            result.Add(new WarehouseStockItemDto
            {
                InventoryItemId = item.Id,
                Sku = item.Sku,
                Name = item.Name,
                CategoryName = item.Category?.Name,
                Barcode = item.Barcode,
                ReorderLevel = item.ReorderLevel,
                NetChange = netChange,
                QuantityOnHand = item.QuantityOnHand,
                IsActive = item.IsActive
            });
        }

        return result;
    }

    private static void AddDelta(Dictionary<Guid, decimal> deltas, Guid itemId, decimal delta)
    {
        deltas.TryGetValue(itemId, out var current);
        deltas[itemId] = current + delta;
    }

    private static bool IsInDateRange(DateTime? opDate, DateTime? from, DateTime? to)
    {
        if (opDate == null) return true; // include if no date on operation
        if (from.HasValue && opDate.Value < from.Value) return false;
        if (to.HasValue && opDate.Value.Date > to.Value.Date) return false;
        return true;
    }
}
