using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAudits.Commands;

public record StockAuditLineInput(
    Guid InventoryItemId,
    decimal CountedQuantity);

public record CreateStockAuditCommand(
    string Number,
    Guid WarehouseId,
    string? Notes,
    DateTime? ReconciledAtUtc,
    IReadOnlyList<StockAuditLineInput> Lines,
    Guid? EmployeeId = null) : IRequest<Guid>;

public class CreateStockAuditCommandHandler : IRequestHandler<CreateStockAuditCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockAuditCommand request, CancellationToken cancellationToken)
    {
        var itemIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var warehouseId = request.WarehouseId;
        var auditDate = request.ReconciledAtUtc ?? DateTime.UtcNow;

        // ASP.NET Core may deserialize date-only strings as Unspecified kind;
        // PostgreSQL requires UTC for timestamp with time zone.
        var utcDate = auditDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(auditDate, DateTimeKind.Utc)
            : auditDate;

        // For storage, use the original date as-is (user-selected date at midnight)
        var storedDate = utcDate;

        // For stock calculation: if midnight (date-only pick), compare by local date
        // so the calculation matches what the user sees in the movement history.
        // Operations whose local date > audit local date are excluded.
        var isDateOnly = utcDate.TimeOfDay == TimeSpan.Zero;
        DateTime calculationCutoffUtc;
        if (isDateOnly)
        {
            // End of the audit's local day, expressed in UTC
            var localNextMidnight = utcDate.ToLocalTime().Date.AddDays(1);
            calculationCutoffUtc = localNextMidnight.ToUniversalTime().AddTicks(-1);
        }
        else
        {
            calculationCutoffUtc = utcDate;
        }

        // Calculate system quantity as of the audit date for each item
        var onHandByItem = new Dictionary<Guid, decimal>();
        foreach (var itemId in itemIds)
        {
            onHandByItem[itemId] = await CalculateStockAtDate(itemId, warehouseId, calculationCutoffUtc, cancellationToken);
        }

        var audit = new StockAudit
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            Notes = request.Notes,
            ReconciledAtUtc = storedDate,
            Status = StockOperationStatus.Draft,
            EmployeeId = request.EmployeeId,
            Lines = request.Lines.Select(l => new StockAuditLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                SystemQuantity = onHandByItem.TryGetValue(l.InventoryItemId, out var onHand) ? onHand : 0m,
                CountedQuantity = l.CountedQuantity
            }).ToList()
        };

        _context.StockAudits.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }

    private async Task<decimal> CalculateStockAtDate(Guid itemId, Guid warehouseId, DateTime asOfUtc, CancellationToken ct)
    {
        decimal stock = 0;

        // Receipts
        var receipts = await _context.StockReceiptLines
            .Where(l => l.InventoryItemId == itemId
                && l.StockReceipt!.WarehouseId == warehouseId
                && l.StockReceipt.Status == StockOperationStatus.Completed
                && l.StockReceipt.ReceivedAtUtc != null
                && l.StockReceipt.ReceivedAtUtc <= asOfUtc)
            .SumAsync(l => l.Quantity, ct);
        stock += receipts;

        // Adjustments
        var adjustments = await _context.StockAdjustmentLines
            .Where(l => l.InventoryItemId == itemId
                && l.StockAdjustment!.WarehouseId == warehouseId
                && l.StockAdjustment.Status == StockOperationStatus.Completed
                && l.StockAdjustment.AdjustedAtUtc != null
                && l.StockAdjustment.AdjustedAtUtc <= asOfUtc)
            .SumAsync(l => l.QuantityChange, ct);
        stock += adjustments;

        // Transfers IN
        var transfersIn = await _context.StockTransferLines
            .Where(l => l.InventoryItemId == itemId
                && l.StockTransfer!.DestinationWarehouseId == warehouseId
                && l.StockTransfer.Status == StockOperationStatus.Completed
                && l.StockTransfer.TransferredAtUtc != null
                && l.StockTransfer.TransferredAtUtc <= asOfUtc)
            .SumAsync(l => l.Quantity, ct);
        stock += transfersIn;

        // Transfers OUT
        var transfersOut = await _context.StockTransferLines
            .Where(l => l.InventoryItemId == itemId
                && l.StockTransfer!.SourceWarehouseId == warehouseId
                && l.StockTransfer.Status == StockOperationStatus.Completed
                && l.StockTransfer.TransferredAtUtc != null
                && l.StockTransfer.TransferredAtUtc <= asOfUtc)
            .SumAsync(l => l.Quantity, ct);
        stock -= transfersOut;

        // Picks
        var pickLines = await _context.PickOrderLines
            .Include(l => l.PickOrder)
            .Where(l => l.InventoryItemId == itemId
                && l.PickOrder!.WarehouseId == warehouseId
                && l.PickOrder.Status == StockOperationStatus.Completed)
            .ToListAsync(ct);
        foreach (var pl in pickLines)
        {
            var pickDate = pl.PickOrder!.PlannedPickDate ?? pl.PickOrder.PickedAtUtc!.Value;
            if (pickDate <= asOfUtc)
                stock -= pl.Quantity;
        }

        // Purchase Orders
        var poLines = await _context.PurchaseOrderLines
            .Include(l => l.PurchaseOrder)
            .Where(l => l.InventoryItemId == itemId
                && l.PurchaseOrder!.WarehouseId == warehouseId
                && l.PurchaseOrder.Status == PurchaseOrderStatus.Received
                && l.PurchaseOrder.ReceivedAtUtc != null)
            .ToListAsync(ct);
        foreach (var pol in poLines)
        {
            if (pol.PurchaseOrder!.ReceivedAtUtc!.Value <= asOfUtc)
                stock += pol.Quantity;
        }

        // Sales Orders
        var soLines = await _context.SalesOrderLines
            .Where(l => l.InventoryItemId == itemId
                && l.SalesOrder!.WarehouseId == warehouseId
                && l.SalesOrder.Status == SalesOrderStatus.Shipped
                && l.SalesOrder.ShippedAtUtc != null
                && l.SalesOrder.ShippedAtUtc <= asOfUtc)
            .SumAsync(l => l.Quantity, ct);
        stock -= soLines;

        return stock;
    }
}
