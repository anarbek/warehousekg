using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.SalesOrders.Commands;

public record SalesOrderLineInput(
    Guid InventoryItemId,
    decimal Quantity,
    decimal UnitPrice);

public record CreateSalesOrderCommand(
    string Number,
    Guid CustomerId,
    Guid? WarehouseId,
    string? Currency,
    DateTime? ExpectedDateUtc,
    string? Notes,
    IReadOnlyList<SalesOrderLineInput> Lines,
    Guid? EmployeeId = null,
    decimal? AmountPlanned = null,
    decimal? AmountPaid = null) : IRequest<Guid>;

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSalesOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var salesOrder = new SalesOrder
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDateUtc = request.ExpectedDateUtc,
            Notes = request.Notes,
            Status = SalesOrderStatus.Draft,
            EmployeeId = request.EmployeeId,
            AmountPlanned = request.AmountPlanned ?? 0,
            AmountPaid = request.AmountPaid ?? 0,
            Lines = request.Lines.Select(l => new SalesOrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return salesOrder.Id;
    }
}

public record UpdateSalesOrderCommand(
    Guid Id,
    string Number,
    Guid CustomerId,
    Guid? WarehouseId,
    string? Currency,
    DateTime? ExpectedDateUtc,
    string? Notes,
    IReadOnlyList<SalesOrderLineInput> Lines) : IRequest<OperationResult>;

public class UpdateSalesOrderCommandHandler : IRequestHandler<UpdateSalesOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalesOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(UpdateSalesOrderCommand request, CancellationToken ct)
    {
        var order = await _context.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (order is null) return OperationResult.NotFound;
        if (order.Status != SalesOrderStatus.Draft) return OperationResult.InvalidState;

        order.Number = request.Number;
        order.CustomerId = request.CustomerId;
        order.WarehouseId = request.WarehouseId;
        order.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency;
        order.ExpectedDateUtc = request.ExpectedDateUtc;
        order.Notes = request.Notes;

        // Remove lines not present in the request, update existing, add new
        var existingById = order.Lines.ToDictionary(l => l.InventoryItemId);
        var requestItemIds = request.Lines.Select(l => l.InventoryItemId).ToHashSet();

        foreach (var (itemId, line) in existingById)
        {
            if (!requestItemIds.Contains(itemId))
            {
                _context.SalesOrderLines.Remove(line);
            }
        }

        foreach (var l in request.Lines)
        {
            if (existingById.TryGetValue(l.InventoryItemId, out var existing))
            {
                existing.Quantity = l.Quantity;
                existing.UnitPrice = l.UnitPrice;
            }
            else
            {
                order.Lines.Add(new SalesOrderLine
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    SalesOrderId = order.Id
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}
