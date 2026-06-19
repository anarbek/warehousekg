using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Personnel.Attendance.Commands;

public record CreateAttendanceCommand(
    Guid EmployeeId, Guid ShiftId, DateTime Date,
    DateTime? ClockInUtc, DateTime? ClockOutUtc,
    AttendanceStatus Status, string? Notes) : IRequest<Guid>;

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateAttendanceCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateAttendanceCommand r, CancellationToken ct)
    {
        var a = new AttendanceRecord
        {
            Id = Guid.NewGuid(), EmployeeId = r.EmployeeId, ShiftId = r.ShiftId,
            Date = r.Date, ClockInUtc = r.ClockInUtc, ClockOutUtc = r.ClockOutUtc,
            Status = r.Status, Notes = r.Notes
        };
        _context.AttendanceRecords.Add(a);
        await _context.SaveChangesAsync(ct);
        return a.Id;
    }
}
