using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.SalesOrders.Commands;

public record ConfirmSalesOrderCommand(Guid Id) : IRequest<OperationResult>;

public class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public ConfirmSalesOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(ConfirmSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        if (order.Status != SalesOrderStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        order.Status = SalesOrderStatus.Confirmed;
        order.ConfirmedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

// Shipping a confirmed order dispatches the goods and decreases stock on hand.
public record ShipSalesOrderCommand(Guid Id) : IRequest<OperationResult>;

public class ShipSalesOrderCommandHandler : IRequestHandler<ShipSalesOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public ShipSalesOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(ShipSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        if (order.Status != SalesOrderStatus.Confirmed)
        {
            return OperationResult.InvalidState;
        }

        var itemIds = order.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in order.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                item.QuantityOnHand -= line.Quantity;
            }
        }

        order.Status = SalesOrderStatus.Shipped;
        order.ShippedAtUtc ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelSalesOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CancelSalesOrderCommandHandler : IRequestHandler<CancelSalesOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelSalesOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        // A shipped order cannot be cancelled; only Draft or Confirmed orders may be.
        if (order.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Confirmed))
        {
            return OperationResult.InvalidState;
        }

        order.Status = SalesOrderStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
