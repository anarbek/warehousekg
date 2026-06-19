using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Vehicles.DriverAssignments.Commands;

public record CreateDriverAssignmentCommand(
    Guid VehicleId, Guid EmployeeId,
    DateTime AssignedFromUtc, DateTime? AssignedToUtc,
    bool IsPrimary) : IRequest<Guid>;

public class CreateDriverAssignmentCommandHandler : IRequestHandler<CreateDriverAssignmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateDriverAssignmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateDriverAssignmentCommand r, CancellationToken ct)
    {
        var a = new VehicleDriverAssignment
        {
            Id = Guid.NewGuid(), VehicleId = r.VehicleId, EmployeeId = r.EmployeeId,
            AssignedFromUtc = DateTime.SpecifyKind(r.AssignedFromUtc, DateTimeKind.Utc),
            AssignedToUtc = r.AssignedToUtc.HasValue ? DateTime.SpecifyKind(r.AssignedToUtc.Value, DateTimeKind.Utc) : null,
            IsPrimary = r.IsPrimary
        };
        _context.VehicleDriverAssignments.Add(a);
        await _context.SaveChangesAsync(ct);
        return a.Id;
    }
}
