using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Application.Features.StockReceipts.Commands;

public record StockReceiptLineInput(
    Guid InventoryItemId,
    Guid? WarehouseLocationId,
    decimal Quantity);

public record CreateStockReceiptCommand(
    string Number,
    Guid WarehouseId,
    string? SupplierReference,
    string? Notes,
    DateTime? ReceivedAtUtc,
    Guid TenantId,
    IReadOnlyList<string> UserRoles,
    IReadOnlyList<StockReceiptLineInput> Lines,
    Guid? EmployeeId = null) : IRequest<Guid>;

public class CreateStockReceiptCommandHandler : IRequestHandler<CreateStockReceiptCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockReceiptCommand request, CancellationToken cancellationToken)
    {
        // Validate back-in-time: if ReceivedAtUtc is in the past, check permission
        var now = DateTime.UtcNow.Date;
        var receivedDate = request.ReceivedAtUtc?.Date ?? now;
        if (receivedDate < now)
        {
            var daysBack = (now - receivedDate).Days;
            var maxDays = int.MaxValue; // default: no backdating allowed unless permission exists

            foreach (var role in request.UserRoles)
            {
                var perm = await _context.TenantPermissions
                    .FirstOrDefaultAsync(p =>
                        p.TenantId == request.TenantId &&
                        p.RoleName == role &&
                        p.Resource == Resources.AddItemsBackInTime,
                        cancellationToken);

                if (perm?.MaxBackdateDays != null)
                {
                    maxDays = Math.Min(maxDays, perm.MaxBackdateDays.Value);
                }
            }

            // If no permission found or daysBack exceeds max, reject
            if (maxDays == int.MaxValue)
            {
                maxDays = 0; // No permission means no backdating
            }

            if (daysBack > maxDays)
            {
                throw new InvalidOperationException(
                    $"Receipt date {receivedDate:yyyy-MM-dd} is {daysBack} days in the past. " +
                    $"Maximum allowed is {maxDays} day(s).");
            }
        }

        var receipt = new StockReceipt
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            SupplierReference = request.SupplierReference,
            Notes = request.Notes,
            ReceivedAtUtc = request.ReceivedAtUtc,
            Status = StockOperationStatus.Draft,
            EmployeeId = request.EmployeeId,
            Lines = request.Lines.Select(l => new StockReceiptLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                WarehouseLocationId = l.WarehouseLocationId,
                Quantity = l.Quantity
            }).ToList()
        };

        _context.StockReceipts.Add(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }
}
