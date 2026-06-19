using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Personnel.Employees.Dtos;

namespace WarehouseKG.Application.Features.Personnel.Employees.Queries;

public record GetEmployeesQuery : IRequest<IReadOnlyList<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetEmployeesQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<EmployeeDto>> Handle(GetEmployeesQuery r, CancellationToken ct)
        => await _context.Employees.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new EmployeeDto
            {
                Id = x.Id, Code = x.Code, FirstName = x.FirstName, LastName = x.LastName,
                MiddleName = x.MiddleName, Email = x.Email, Phone = x.Phone,
                HireDate = x.HireDate, TerminationDate = x.TerminationDate,
                PositionId = x.PositionId, PositionName = x.Position != null ? x.Position.Name : null,
                DepartmentId = x.DepartmentId, DepartmentName = x.Department != null ? x.Department.Name : null,
                ApplicationUserId = x.ApplicationUserId,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
}

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    private readonly IApplicationDbContext _context;
    public GetEmployeeByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<EmployeeDto?> Handle(GetEmployeeByIdQuery r, CancellationToken ct)
        => await _context.Employees.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new EmployeeDto
            {
                Id = x.Id, Code = x.Code, FirstName = x.FirstName, LastName = x.LastName,
                MiddleName = x.MiddleName, Email = x.Email, Phone = x.Phone,
                HireDate = x.HireDate, TerminationDate = x.TerminationDate,
                PositionId = x.PositionId, PositionName = x.Position != null ? x.Position.Name : null,
                DepartmentId = x.DepartmentId, DepartmentName = x.Department != null ? x.Department.Name : null,
                ApplicationUserId = x.ApplicationUserId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
}

public record GetEmployeeDetailQuery(Guid Id) : IRequest<EmployeeDetailDto?>;

public class GetEmployeeDetailQueryHandler : IRequestHandler<GetEmployeeDetailQuery, EmployeeDetailDto?>
{
    private readonly IApplicationDbContext _context;
    public GetEmployeeDetailQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<EmployeeDetailDto?> Handle(GetEmployeeDetailQuery r, CancellationToken ct)
    {
        var e = await _context.Employees.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new EmployeeDetailDto
            {
                Id = x.Id, Code = x.Code, FirstName = x.FirstName, LastName = x.LastName,
                MiddleName = x.MiddleName, Email = x.Email, Phone = x.Phone,
                HireDate = x.HireDate, TerminationDate = x.TerminationDate,
                PositionId = x.PositionId, PositionName = x.Position != null ? x.Position.Name : null,
                DepartmentId = x.DepartmentId, DepartmentName = x.Department != null ? x.Department.Name : null,
                ApplicationUserId = x.ApplicationUserId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
        if (e is null) return null;

        e.ShiftAssignments = await _context.EmployeeShiftAssignments
            .AsNoTracking().Where(a => a.EmployeeId == r.Id)
            .Select(a => new ShiftAssignmentDto
            {
                Id = a.Id, ShiftId = a.ShiftId, ShiftName = a.Shift!.Name,
                EffectiveFromUtc = a.EffectiveFromUtc, EffectiveToUtc = a.EffectiveToUtc
            }).ToListAsync(ct);

        e.WarehouseAssignments = await _context.EmployeeWarehouseAssignments
            .AsNoTracking().Where(a => a.EmployeeId == r.Id)
            .Select(a => new WarehouseAssignmentDto
            {
                Id = a.Id, WarehouseId = a.WarehouseId, WarehouseName = a.Warehouse!.Name,
                IsPrimary = a.IsPrimary
            }).ToListAsync(ct);

        return e;
    }
}
