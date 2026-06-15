using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Warehouses.Commands;

public record UpdateWarehouseCommand(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    bool IsActive) : IRequest<bool>;

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse is null)
        {
            return false;
        }

        warehouse.Code = request.Code;
        warehouse.Name = request.Name;
        warehouse.Address = request.Address;
        warehouse.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
