using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Customers.Dtos;

namespace WarehouseKG.Application.Features.Customers.Queries;

public record GetCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>;

public class GetCustomersQueryHandler
    : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactName = c.ContactName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                TaxId = c.TaxId,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

public class GetCustomerByIdQueryHandler
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactName = c.ContactName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                TaxId = c.TaxId,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
