using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Attendance.Commands;

public record DeleteAttendanceCommand(Guid Id) : IRequest<bool>;

public class DeleteAttendanceCommandHandler : IRequestHandler<DeleteAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteAttendanceCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteAttendanceCommand r, CancellationToken ct)
    {
        var a = await _context.AttendanceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (a is null) return false;
        _context.AttendanceRecords.Remove(a);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
