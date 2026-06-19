using MediatR;
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
    Guid? EmployeeId = null) : IRequest<Guid>;

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
