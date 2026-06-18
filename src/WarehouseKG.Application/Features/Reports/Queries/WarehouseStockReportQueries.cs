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

        // ─── Load all completed operations at this warehouse ──────────
        var receipts = await _context.StockReceipts
            .Where(r => r.WarehouseId == warehouseId && r.Status == StockOperationStatus.Completed)
            .Include(r => r.Lines).ToListAsync(cancellationToken);
        var adjustments = await _context.StockAdjustments
            .Where(a => a.WarehouseId == warehouseId && a.Status == StockOperationStatus.Completed)
            .Include(a => a.Lines).ToListAsync(cancellationToken);
        var audits = await _context.StockAudits
            .Where(a => a.WarehouseId == warehouseId && a.Status == StockOperationStatus.Completed)
            .Include(a => a.Lines).ToListAsync(cancellationToken);
        var transfersIn = await _context.StockTransfers
            .Where(t => t.DestinationWarehouseId == warehouseId && t.Status == StockOperationStatus.Completed)
            .Include(t => t.Lines).ToListAsync(cancellationToken);
        var transfersOut = await _context.StockTransfers
            .Where(t => t.SourceWarehouseId == warehouseId && t.Status == StockOperationStatus.Completed)
            .Include(t => t.Lines).ToListAsync(cancellationToken);
        var picks = await _context.PickOrders
            .Where(p => p.WarehouseId == warehouseId && p.Status == StockOperationStatus.Completed)
            .Include(p => p.Lines).ToListAsync(cancellationToken);
        var pos = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .Where(po => po.WarehouseId == warehouseId && po.Status == PurchaseOrderStatus.Received)
            .ToListAsync(cancellationToken);
        var sos = await _context.SalesOrders
            .Include(so => so.Lines)
            .Where(so => so.WarehouseId == warehouseId && so.Status == SalesOrderStatus.Shipped)
            .ToListAsync(cancellationToken);

        // ─── Build per-item event lists mirroring ItemMovementHistoryQueries ──
        var itemEvents = new Dictionary<Guid, List<(DateTime Ts, string Op, decimal Delta, DateTime Created)>>();
        void AddEvent(Guid itemId, DateTime ts, string op, decimal delta, DateTime created)
        {
            if (!itemEvents.TryGetValue(itemId, out var list))
                itemEvents[itemId] = list = new List<(DateTime, string, decimal, DateTime)>();
            list.Add((ts, op, delta, created));
        }

        foreach (var r in receipts)
            foreach (var l in r.Lines)
                if (r.ReceivedAtUtc != null)
                    AddEvent(l.InventoryItemId, r.ReceivedAtUtc.Value, "Поступление", l.Quantity, r.CreatedAt);

        foreach (var a in adjustments)
            foreach (var l in a.Lines)
                if (a.AdjustedAtUtc != null)
                    AddEvent(l.InventoryItemId, a.AdjustedAtUtc.Value, "Корректировка", l.QuantityChange, a.CreatedAt);

        foreach (var a in audits)
            foreach (var l in a.Lines)
                if (a.ReconciledAtUtc != null)
                    AddEvent(l.InventoryItemId, a.ReconciledAtUtc.Value, "Аудит", l.CountedQuantity - l.SystemQuantity, a.CreatedAt);

        foreach (var t in transfersIn)
            foreach (var l in t.Lines)
                if (t.TransferredAtUtc != null)
                    AddEvent(l.InventoryItemId, t.TransferredAtUtc.Value, "Перемещение (приход)", l.Quantity, t.CreatedAt);

        foreach (var t in transfersOut)
            foreach (var l in t.Lines)
                if (t.TransferredAtUtc != null)
                    AddEvent(l.InventoryItemId, t.TransferredAtUtc.Value, "Перемещение (расход)", -l.Quantity, t.CreatedAt);

        foreach (var p in picks)
        {
            var pickDate = p.PlannedPickDate ?? p.PickedAtUtc;
            if (pickDate != null)
                foreach (var l in p.Lines)
                    AddEvent(l.InventoryItemId, pickDate.Value, "Сборка", -l.Quantity, p.CreatedAt);
        }

        foreach (var po in pos)
            foreach (var l in po.Lines)
                if (po.ReceivedAtUtc != null)
                    AddEvent(l.InventoryItemId, po.ReceivedAtUtc.Value, "Закупка", l.Quantity, po.CreatedAt);

        foreach (var so in sos)
            foreach (var l in so.Lines)
                if (so.ShippedAtUtc != null)
                    AddEvent(l.InventoryItemId, so.ShippedAtUtc.Value, "Продажа", -l.Quantity, so.CreatedAt);

        // ─── Sort each item's events the same way as the movement history ──
        // Local date, audits last, then UTC timestamp, then CreatedAt
        foreach (var kv in itemEvents)
        {
            kv.Value.Sort((a, b) =>
            {
                var dateCmp = a.Ts.ToLocalTime().Date.CompareTo(b.Ts.ToLocalTime().Date);
                if (dateCmp != 0) return dateCmp;
                var auditCmp = (a.Op == "Аудит" ? 1 : 0).CompareTo(b.Op == "Аудит" ? 1 : 0);
                if (auditCmp != 0) return auditCmp;
                var tsCmp = a.Ts.CompareTo(b.Ts);
                if (tsCmp != 0) return tsCmp;
                return a.Created.CompareTo(b.Created);
            });
        }

        // ─── Compute Oborot (net change in date range) and Vsego (running balance at "to") ──
        var oborotByItem = new Dictionary<Guid, decimal>();
        var vsegoByItem = new Dictionary<Guid, decimal>();

        foreach (var (itemId, events) in itemEvents)
        {
            decimal balance = 0;
            decimal oborot = 0;
            decimal? vsego = null;

            foreach (var e in events)
            {
                balance += e.Delta;

                // Check if this event falls within the date range
                var localDate = e.Ts.ToLocalTime().Date;
                bool inRange = true;
                if (request.DateFrom.HasValue && localDate < request.DateFrom.Value.Date) inRange = false;
                if (request.DateTo.HasValue && localDate > request.DateTo.Value.Date) inRange = false;

                if (inRange && e.Op != "Аудит")
                    oborot += e.Delta;

                // Track running balance at the "to" date
                if (request.DateTo.HasValue && localDate <= request.DateTo.Value.Date)
                    vsego = balance;
            }

            // If no "to" date, Vsego = ending balance
            if (!request.DateTo.HasValue)
                vsego = balance;

            oborotByItem[itemId] = oborot;
            vsegoByItem[itemId] = vsego ?? balance;
        }

        // ─── Enrich with item details ─────────────────────────────────────
        var itemIds = itemEvents.Keys.ToList();
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var result = new List<WarehouseStockItemDto>();
        foreach (var (itemId, events) in itemEvents.OrderByDescending(kv => oborotByItem[kv.Key]))
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
                NetChange = oborotByItem[itemId],
                QuantityOnHand = vsegoByItem[itemId],
                IsActive = item.IsActive
            });
        }

        return result;
    }

}
