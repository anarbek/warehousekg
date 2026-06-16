using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.UnitsOfMeasure.Commands;

public record CreateUnitOfMeasureCommand(
    string Code,
    string Name,
    string? Description = null) : IRequest<Guid>;

public class CreateUnitOfMeasureCommandHandler : IRequestHandler<CreateUnitOfMeasureCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateUnitOfMeasureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var uom = new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description
        };

        _context.UnitsOfMeasure.Add(uom);
        await _context.SaveChangesAsync(cancellationToken);

        return uom.Id;
    }
}

public record UpdateUnitOfMeasureCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description) : IRequest<bool>;

public class UpdateUnitOfMeasureCommandHandler : IRequestHandler<UpdateUnitOfMeasureCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateUnitOfMeasureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var uom = await _context.UnitsOfMeasure
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (uom is null)
            return false;

        uom.Code = request.Code;
        uom.Name = request.Name;
        uom.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteUnitOfMeasureCommand(Guid Id) : IRequest<bool>;

public class DeleteUnitOfMeasureCommandHandler : IRequestHandler<DeleteUnitOfMeasureCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteUnitOfMeasureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var uom = await _context.UnitsOfMeasure
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (uom is null)
            return false;

        _context.UnitsOfMeasure.Remove(uom);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
