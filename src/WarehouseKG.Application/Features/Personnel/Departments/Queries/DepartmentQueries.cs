using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Personnel.Departments.Dtos;

namespace WarehouseKG.Application.Features.Personnel.Departments.Queries;

public record GetDepartmentsQuery : IRequest<IReadOnlyList<DepartmentDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;
    public GetDepartmentsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<DepartmentDto>> Handle(GetDepartmentsQuery r, CancellationToken ct)
        => await _context.Departments.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new DepartmentDto { Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, IsActive = x.IsActive })
            .ToListAsync(ct);
}

public record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto?>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IApplicationDbContext _context;
    public GetDepartmentByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery r, CancellationToken ct)
        => await _context.Departments.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new DepartmentDto { Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, IsActive = x.IsActive })
            .FirstOrDefaultAsync(ct);
}
