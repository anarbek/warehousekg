using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockTransfers.Commands;

public record StockTransferLineInput(
    Guid InventoryItemId,
    decimal Quantity);

public record CreateStockTransferCommand(
    string Number,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string? Notes,
    DateTime? TransferredAtUtc,
    IReadOnlyList<StockTransferLineInput> Lines,
    Guid? EmployeeId = null) : IRequest<Guid>;

public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = new StockTransfer
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            SourceWarehouseId = request.SourceWarehouseId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            Notes = request.Notes,
            TransferredAtUtc = request.TransferredAtUtc,
            Status = StockOperationStatus.Draft,
            EmployeeId = request.EmployeeId,
            Lines = request.Lines.Select(l => new StockTransferLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity
            }).ToList()
        };

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }
}
