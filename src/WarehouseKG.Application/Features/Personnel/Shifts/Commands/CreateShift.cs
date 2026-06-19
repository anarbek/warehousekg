using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Personnel.Shifts.Commands;

public record CreateShiftCommand(string Code, string Name, TimeOnly StartTime, TimeOnly EndTime, bool IsActive = true) : IRequest<Guid>;

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateShiftCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateShiftCommand r, CancellationToken ct)
    {
        var s = new Shift { Id = Guid.NewGuid(), Code = r.Code, Name = r.Name, StartTime = r.StartTime, EndTime = r.EndTime, IsActive = r.IsActive };
        _context.Shifts.Add(s);
        await _context.SaveChangesAsync(ct);
        return s.Id;
    }
}
