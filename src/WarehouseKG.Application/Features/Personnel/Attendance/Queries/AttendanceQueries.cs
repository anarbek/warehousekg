using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Personnel.Attendance.Dtos;

namespace WarehouseKG.Application.Features.Personnel.Attendance.Queries;

public record GetAttendanceQuery(DateTime? From, DateTime? To) : IRequest<IReadOnlyList<AttendanceDto>>;

public class GetAttendanceQueryHandler : IRequestHandler<GetAttendanceQuery, IReadOnlyList<AttendanceDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAttendanceQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<AttendanceDto>> Handle(GetAttendanceQuery r, CancellationToken ct)
    {
        var query = _context.AttendanceRecords.AsNoTracking();
        if (r.From.HasValue) query = query.Where(x => x.Date >= r.From.Value);
        if (r.To.HasValue) query = query.Where(x => x.Date <= r.To.Value);
        return await query.OrderByDescending(x => x.Date).ThenBy(x => x.Employee!.LastName)
            .Select(x => new AttendanceDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee!.LastName + " " + x.Employee.FirstName,
                ShiftId = x.ShiftId, ShiftName = x.Shift!.Name,
                Date = x.Date, ClockInUtc = x.ClockInUtc, ClockOutUtc = x.ClockOutUtc,
                Status = x.Status, Notes = x.Notes
            }).ToListAsync(ct);
    }
}

public record GetAttendanceByIdQuery(Guid Id) : IRequest<AttendanceDto?>;

public class GetAttendanceByIdQueryHandler : IRequestHandler<GetAttendanceByIdQuery, AttendanceDto?>
{
    private readonly IApplicationDbContext _context;
    public GetAttendanceByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AttendanceDto?> Handle(GetAttendanceByIdQuery r, CancellationToken ct)
        => await _context.AttendanceRecords.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new AttendanceDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee!.LastName + " " + x.Employee.FirstName,
                ShiftId = x.ShiftId, ShiftName = x.Shift!.Name,
                Date = x.Date, ClockInUtc = x.ClockInUtc, ClockOutUtc = x.ClockOutUtc,
                Status = x.Status, Notes = x.Notes
            }).FirstOrDefaultAsync(ct);
}
