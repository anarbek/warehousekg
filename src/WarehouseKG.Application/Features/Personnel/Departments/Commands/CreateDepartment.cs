using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Personnel.Departments.Commands;

public record CreateDepartmentCommand(string Code, string Name, string? Description, bool IsActive = true) : IRequest<Guid>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateDepartmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateDepartmentCommand r, CancellationToken ct)
    {
        var d = new Department { Id = Guid.NewGuid(), Code = r.Code, Name = r.Name, Description = r.Description, IsActive = r.IsActive };
        _context.Departments.Add(d);
        await _context.SaveChangesAsync(ct);
        return d.Id;
    }
}
