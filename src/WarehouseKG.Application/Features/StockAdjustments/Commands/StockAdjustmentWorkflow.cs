using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAdjustments.Commands;

// Completing an adjustment applies each line's signed QuantityChange to the tenant-wide
// InventoryItem.QuantityOnHand. Per-location stock is a planned enhancement (see docs/02-Database-Schema).
public record CompleteStockAdjustmentCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteStockAdjustmentCommandHandler : IRequestHandler<CompleteStockAdjustmentCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteStockAdjustmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompleteStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (adjustment is null)
        {
            return OperationResult.NotFound;
        }

        if (adjustment.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        var itemIds = adjustment.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in adjustment.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                item.QuantityOnHand += line.QuantityChange;
            }
        }

        adjustment.Status = StockOperationStatus.Completed;
        adjustment.AdjustedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelStockAdjustmentCommand(Guid Id) : IRequest<OperationResult>;

public class CancelStockAdjustmentCommandHandler : IRequestHandler<CancelStockAdjustmentCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelStockAdjustmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (adjustment is null)
        {
            return OperationResult.NotFound;
        }

        if (adjustment.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        adjustment.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
