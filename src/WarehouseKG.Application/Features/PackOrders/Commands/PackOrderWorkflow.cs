using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PackOrders.Commands;

// Packing operates on already-picked goods, so completing a pack order does not
// change stock-on-hand; it only advances the document status.
public record CompletePackOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CompletePackOrderCommandHandler : IRequestHandler<CompletePackOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompletePackOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompletePackOrderCommand request, CancellationToken cancellationToken)
    {
        var packOrder = await _context.PackOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (packOrder is null)
        {
            return OperationResult.NotFound;
        }

        if (packOrder.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        packOrder.Status = StockOperationStatus.Completed;
        packOrder.PackedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelPackOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CancelPackOrderCommandHandler : IRequestHandler<CancelPackOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelPackOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelPackOrderCommand request, CancellationToken cancellationToken)
    {
        var packOrder = await _context.PackOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (packOrder is null)
        {
            return OperationResult.NotFound;
        }

        if (packOrder.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        packOrder.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
