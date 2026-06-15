using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PackOrders.Commands;

public record PackOrderLineInput(
    Guid InventoryItemId,
    decimal Quantity,
    string? PackageLabel);

public record CreatePackOrderCommand(
    string Number,
    Guid WarehouseId,
    Guid? PickOrderId,
    string? Notes,
    IReadOnlyList<PackOrderLineInput> Lines) : IRequest<Guid>;

public class CreatePackOrderCommandHandler : IRequestHandler<CreatePackOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePackOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePackOrderCommand request, CancellationToken cancellationToken)
    {
        var packOrder = new PackOrder
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            PickOrderId = request.PickOrderId,
            Notes = request.Notes,
            Status = StockOperationStatus.Draft,
            Lines = request.Lines.Select(l => new PackOrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity,
                PackageLabel = l.PackageLabel
            }).ToList()
        };

        _context.PackOrders.Add(packOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return packOrder.Id;
    }
}
