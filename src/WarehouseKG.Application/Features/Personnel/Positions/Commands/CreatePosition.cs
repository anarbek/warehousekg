using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Personnel.Positions.Commands;

public record CreatePositionCommand(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true) : IRequest<Guid>;

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);
        return position.Id;
    }
}
