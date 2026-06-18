using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAudits.Commands;

// Completing an audit reconciles the system: each item's tenant-wide QuantityOnHand is set to the
// physically counted quantity, applying the variance. Per-location stock is a planned enhancement
// (see docs/02-Database-Schema).
public record CompleteStockAuditCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteStockAuditCommandHandler : IRequestHandler<CompleteStockAuditCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompleteStockAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = await _context.StockAudits
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (audit is null)
        {
            return OperationResult.NotFound;
        }

        if (audit.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        var itemIds = audit.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in audit.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                // Apply the variance (counted − system) instead of overwriting QOH,
                // so that operations at other warehouses are preserved.
                item.QuantityOnHand += line.CountedQuantity - line.SystemQuantity;
            }
        }

        audit.Status = StockOperationStatus.Completed;
        audit.ReconciledAtUtc ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelStockAuditCommand(Guid Id) : IRequest<OperationResult>;

public class CancelStockAuditCommandHandler : IRequestHandler<CancelStockAuditCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelStockAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = await _context.StockAudits
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (audit is null)
        {
            return OperationResult.NotFound;
        }

        if (audit.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        audit.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
