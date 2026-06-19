using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Personnel.Attendance.Commands;

public record UpdateAttendanceCommand(
    Guid Id, Guid EmployeeId, Guid ShiftId, DateTime Date,
    DateTime? ClockInUtc, DateTime? ClockOutUtc,
    AttendanceStatus Status, string? Notes) : IRequest<bool>;

public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateAttendanceCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateAttendanceCommand r, CancellationToken ct)
    {
        var a = await _context.AttendanceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (a is null) return false;
        a.EmployeeId = r.EmployeeId; a.ShiftId = r.ShiftId; a.Date = r.Date;
        a.ClockInUtc = r.ClockInUtc; a.ClockOutUtc = r.ClockOutUtc;
        a.Status = r.Status; a.Notes = r.Notes;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
