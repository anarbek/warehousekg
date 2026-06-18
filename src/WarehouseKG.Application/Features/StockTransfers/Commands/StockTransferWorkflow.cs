using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockTransfers.Commands;

// InventoryItem tracks a single tenant-wide QuantityOnHand (not per-warehouse), so completing a
// transfer between warehouses does not change the total on hand; it only advances the status.
// Per-location stock levels are a planned enhancement (see docs/02-Database-Schema).
public record CompleteStockTransferCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteStockTransferCommandHandler : IRequestHandler<CompleteStockTransferCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteStockTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompleteStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transfer is null)
        {
            return OperationResult.NotFound;
        }

        if (transfer.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        transfer.Status = StockOperationStatus.Completed;
        transfer.TransferredAtUtc ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelStockTransferCommand(Guid Id) : IRequest<OperationResult>;

public class CancelStockTransferCommandHandler : IRequestHandler<CancelStockTransferCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelStockTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transfer is null)
        {
            return OperationResult.NotFound;
        }

        if (transfer.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        transfer.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
