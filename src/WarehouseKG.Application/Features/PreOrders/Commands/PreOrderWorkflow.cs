using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PreOrders.Commands;

public record SubmitPreOrderCommand(Guid Id) : IRequest<OperationResult>;

public class SubmitPreOrderCommandHandler : IRequestHandler<SubmitPreOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public SubmitPreOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(SubmitPreOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null) return OperationResult.NotFound;
        if (preOrder.Status != PreOrderStatus.Draft) return OperationResult.InvalidState;
        if (preOrder.Lines.Count == 0) return OperationResult.InvalidState;

        preOrder.Status = PreOrderStatus.Submitted;
        preOrder.SubmittedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record ApprovePreOrderCommand(Guid Id) : IRequest<OperationResult>;

public class ApprovePreOrderCommandHandler : IRequestHandler<ApprovePreOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public ApprovePreOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(ApprovePreOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null) return OperationResult.NotFound;
        if (preOrder.Status != PreOrderStatus.Submitted) return OperationResult.InvalidState;

        preOrder.Status = PreOrderStatus.Approved;
        preOrder.ApprovedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record RejectPreOrderCommand(Guid Id, string? Reason) : IRequest<OperationResult>;

public class RejectPreOrderCommandHandler : IRequestHandler<RejectPreOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public RejectPreOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(RejectPreOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null) return OperationResult.NotFound;
        if (preOrder.Status != PreOrderStatus.Submitted) return OperationResult.InvalidState;

        preOrder.Status = PreOrderStatus.Rejected;
        preOrder.RejectedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            preOrder.Notes = string.IsNullOrEmpty(preOrder.Notes)
                ? $"Rejected: {request.Reason}"
                : $"{preOrder.Notes}\nRejected: {request.Reason}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record ConvertPreOrderToSalesOrderCommand(Guid Id) : IRequest<ConvertPreOrderResult>;

public record ConvertPreOrderResult(Guid SalesOrderId, bool Success, string? Error);

public class ConvertPreOrderToSalesOrderCommandHandler
    : IRequestHandler<ConvertPreOrderToSalesOrderCommand, ConvertPreOrderResult>
{
    private readonly IApplicationDbContext _context;

    public ConvertPreOrderToSalesOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ConvertPreOrderResult> Handle(
        ConvertPreOrderToSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null)
            return new ConvertPreOrderResult(Guid.Empty, false, "Pre-order not found.");

        if (preOrder.Status != PreOrderStatus.Approved)
            return new ConvertPreOrderResult(Guid.Empty, false, "Only approved pre-orders can be converted.");

        if (preOrder.ConvertedSalesOrderId.HasValue)
            return new ConvertPreOrderResult(Guid.Empty, false, "Pre-order has already been converted.");

        var salesOrder = new SalesOrder
        {
            Id = Guid.NewGuid(),
            Number = $"SO-{preOrder.Number}",
            CustomerId = preOrder.CustomerId,
            WarehouseId = preOrder.WarehouseId,
            Currency = preOrder.Currency,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDateUtc = preOrder.ExpectedDateUtc,
            Notes = $"Converted from pre-order {preOrder.Number}",
            Status = SalesOrderStatus.Draft,
            Lines = preOrder.Lines.Select(l => new SalesOrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
            }).ToList()
        };

        _context.SalesOrders.Add(salesOrder);

        preOrder.Status = PreOrderStatus.Converted;
        preOrder.ConvertedAtUtc = DateTime.UtcNow;
        preOrder.ConvertedSalesOrderId = salesOrder.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return new ConvertPreOrderResult(salesOrder.Id, true, null);
    }
}
