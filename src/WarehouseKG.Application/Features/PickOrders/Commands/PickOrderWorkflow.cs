using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PickOrders.Commands;

public record CompletePickOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CompletePickOrderCommandHandler : IRequestHandler<CompletePickOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompletePickOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompletePickOrderCommand request, CancellationToken cancellationToken)
    {
        var pickOrder = await _context.PickOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pickOrder is null)
        {
            return OperationResult.NotFound;
        }

        if (pickOrder.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        // Picking removes stock on hand for each picked item.
        var itemIds = pickOrder.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in pickOrder.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                item.QuantityOnHand -= line.Quantity;
            }
        }

        pickOrder.Status = StockOperationStatus.Completed;
        pickOrder.PickedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelPickOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CancelPickOrderCommandHandler : IRequestHandler<CancelPickOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelPickOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelPickOrderCommand request, CancellationToken cancellationToken)
    {
        var pickOrder = await _context.PickOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pickOrder is null)
        {
            return OperationResult.NotFound;
        }

        if (pickOrder.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        pickOrder.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
