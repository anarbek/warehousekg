using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Positions.Commands;

public record UpdatePositionCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive) : IRequest<bool>;

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdatePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (position is null) return false;

        position.Code = request.Code;
        position.Name = request.Name;
        position.Description = request.Description;
        position.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
