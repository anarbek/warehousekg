using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PickOrders.Commands;

public record PickOrderLineInput(
    Guid InventoryItemId,
    Guid? WarehouseLocationId,
    decimal Quantity);

public record CreatePickOrderCommand(
    string Number,
    Guid WarehouseId,
    string? Reference,
    string? Notes,
    DateTime? PlannedPickDate,
    IReadOnlyList<PickOrderLineInput> Lines,
    Guid? EmployeeId = null) : IRequest<Guid>;

public class CreatePickOrderCommandHandler : IRequestHandler<CreatePickOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePickOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePickOrderCommand request, CancellationToken cancellationToken)
    {
        var pickOrder = new PickOrder
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            Reference = request.Reference,
            Notes = request.Notes,
            PlannedPickDate = request.PlannedPickDate,
            Status = StockOperationStatus.Draft,
            EmployeeId = request.EmployeeId,
            Lines = request.Lines.Select(l => new PickOrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                WarehouseLocationId = l.WarehouseLocationId,
                Quantity = l.Quantity
            }).ToList()
        };

        _context.PickOrders.Add(pickOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return pickOrder.Id;
    }
}
