using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Suppliers.Dtos;

namespace WarehouseKG.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<IReadOnlyList<SupplierDto>>;

public class GetSuppliersQueryHandler
    : IRequestHandler<GetSuppliersQuery, IReadOnlyList<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSuppliersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SupplierDto>> Handle(
        GetSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactName = s.ContactName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxId = s.TaxId,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierDto?>;

public class GetSupplierByIdQueryHandler
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSupplierByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto?> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactName = s.ContactName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxId = s.TaxId,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
