using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Reports.Queries;

public record GetItemMovementHistoryQuery(
    Guid ItemId,
    Guid WarehouseId) : IRequest<IReadOnlyList<ItemMovementDto>>;

public class GetItemMovementHistoryQueryHandler
    : IRequestHandler<GetItemMovementHistoryQuery, IReadOnlyList<ItemMovementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemMovementHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ItemMovementDto>> Handle(
        GetItemMovementHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var itemId = request.ItemId;
        var warehouseId = request.WarehouseId;
        var events = new List<(DateTime TimestampUtc, string OperationType, string DocNumber, Guid DocId, decimal Delta, string? Notes)>();

        // 1. StockReceipts
        var receipts = await _context.StockReceiptLines
            .Include(l => l.StockReceipt)
            .Where(l => l.InventoryItemId == itemId
                && l.StockReceipt!.WarehouseId == warehouseId
                && l.StockReceipt.Status == StockOperationStatus.Completed
                && l.StockReceipt.ReceivedAtUtc != null)
            .Select(l => new { l.StockReceipt!.Id, l.StockReceipt!.ReceivedAtUtc, l.StockReceipt.Number, l.Quantity, l.StockReceipt!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(receipts.Select(r =>
            (r.ReceivedAtUtc!.Value, "Поступление", r.Number, r.Id, r.Quantity, r.Notes)));

        // 2. StockAdjustments
        var adjustments = await _context.StockAdjustmentLines
            .Include(l => l.StockAdjustment)
            .Where(l => l.InventoryItemId == itemId
                && l.StockAdjustment!.WarehouseId == warehouseId
                && l.StockAdjustment.Status == StockOperationStatus.Completed
                && l.StockAdjustment.AdjustedAtUtc != null)
            .Select(l => new { l.StockAdjustment!.Id, l.StockAdjustment!.AdjustedAtUtc, l.StockAdjustment.Number, l.QuantityChange, Reason = l.StockAdjustment.Reason.ToString(), l.StockAdjustment!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(adjustments.Select(a =>
            (a.AdjustedAtUtc!.Value, $"Корректировка ({a.Reason})", a.Number, a.Id, a.QuantityChange, a.Notes)));

        // 3. StockAudits
        var audits = await _context.StockAuditLines
            .Include(l => l.StockAudit)
            .Where(l => l.InventoryItemId == itemId
                && l.StockAudit!.WarehouseId == warehouseId
                && l.StockAudit.Status == StockOperationStatus.Completed
                && l.StockAudit.ReconciledAtUtc != null)
            .Select(l => new { l.StockAudit!.Id, l.StockAudit!.ReconciledAtUtc, l.StockAudit.Number, l.CountedQuantity, l.SystemQuantity, l.StockAudit!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(audits.Select(a =>
            (a.ReconciledAtUtc!.Value, "Аудит", a.Number, a.Id, a.CountedQuantity - a.SystemQuantity, a.Notes)));

        // 4. StockTransfers — TO this warehouse
        var transfersIn = await _context.StockTransferLines
            .Include(l => l.StockTransfer)
            .Where(l => l.InventoryItemId == itemId
                && l.StockTransfer!.DestinationWarehouseId == warehouseId
                && l.StockTransfer.Status == StockOperationStatus.Completed
                && l.StockTransfer.TransferredAtUtc != null)
            .Select(l => new { l.StockTransfer!.Id, l.StockTransfer!.TransferredAtUtc, l.StockTransfer.Number, l.Quantity, l.StockTransfer!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(transfersIn.Select(t =>
            (t.TransferredAtUtc!.Value, "Перемещение (приход)", t.Number, t.Id, t.Quantity, t.Notes)));

        // 5. StockTransfers — FROM this warehouse
        var transfersOut = await _context.StockTransferLines
            .Include(l => l.StockTransfer)
            .Where(l => l.InventoryItemId == itemId
                && l.StockTransfer!.SourceWarehouseId == warehouseId
                && l.StockTransfer.Status == StockOperationStatus.Completed
                && l.StockTransfer.TransferredAtUtc != null)
            .Select(l => new { l.StockTransfer!.Id, l.StockTransfer!.TransferredAtUtc, l.StockTransfer.Number, l.Quantity, l.StockTransfer!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(transfersOut.Select(t =>
            (t.TransferredAtUtc!.Value, "Перемещение (расход)", t.Number, t.Id, -t.Quantity, t.Notes)));

        // 6. PickOrders
        var picks = await _context.PickOrderLines
            .Include(l => l.PickOrder)
            .Where(l => l.InventoryItemId == itemId
                && l.PickOrder!.WarehouseId == warehouseId
                && l.PickOrder.Status == StockOperationStatus.Completed
                && l.PickOrder.PickedAtUtc != null)
            .Select(l => new { l.PickOrder!.Id, l.PickOrder!.PickedAtUtc, l.PickOrder.Number, l.Quantity, l.PickOrder!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(picks.Select(p =>
            (p.PickedAtUtc!.Value, "Сборка", p.Number, p.Id, -p.Quantity, p.Notes)));

        // 7. PurchaseOrders
        var pos = await _context.PurchaseOrderLines
            .Include(l => l.PurchaseOrder)
            .Where(l => l.InventoryItemId == itemId
                && l.PurchaseOrder!.WarehouseId == warehouseId
                && l.PurchaseOrder.Status == PurchaseOrderStatus.Received
                && l.PurchaseOrder.ReceivedAtUtc != null)
            .Select(l => new { l.PurchaseOrder!.Id, l.PurchaseOrder!.ReceivedAtUtc, l.PurchaseOrder.Number, l.Quantity, l.PurchaseOrder!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(pos.Select(p =>
            (p.ReceivedAtUtc!.Value, "Закупка", p.Number, p.Id, p.Quantity, p.Notes)));

        // 8. SalesOrders
        var sos = await _context.SalesOrderLines
            .Include(l => l.SalesOrder)
            .Where(l => l.InventoryItemId == itemId
                && l.SalesOrder!.WarehouseId == warehouseId
                && l.SalesOrder.Status == SalesOrderStatus.Shipped
                && l.SalesOrder.ShippedAtUtc != null)
            .Select(l => new { l.SalesOrder!.Id, l.SalesOrder!.ShippedAtUtc, l.SalesOrder.Number, l.Quantity, l.SalesOrder!.Notes })
            .ToListAsync(cancellationToken);

        events.AddRange(sos.Select(s =>
            (s.ShippedAtUtc!.Value, "Продажа", s.Number, s.Id, -s.Quantity, s.Notes)));

        // Sort chronologically and compute running balance
        var sorted = events.OrderBy(e => e.TimestampUtc).ToList();
        var result = new List<ItemMovementDto>();
        decimal balance = 0;

        foreach (var e in sorted)
        {
            balance += e.Delta;
            result.Add(new ItemMovementDto
            {
                TimestampUtc = e.TimestampUtc,
                OperationType = e.OperationType,
                DocumentNumber = e.DocNumber,
                DocumentId = e.DocId,
                QuantityChange = e.Delta,
                Notes = e.Notes,
                RunningBalance = balance
            });
        }

        return result;
    }
}
