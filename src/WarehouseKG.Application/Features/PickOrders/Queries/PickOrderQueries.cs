using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PickOrders.Dtos;

namespace WarehouseKG.Application.Features.PickOrders.Queries;

public record GetPickOrdersQuery : IRequest<IReadOnlyList<PickOrderSummaryDto>>;

public class GetPickOrdersQueryHandler
    : IRequestHandler<GetPickOrdersQuery, IReadOnlyList<PickOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPickOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PickOrderSummaryDto>> Handle(
        GetPickOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PickOrders
            .AsNoTracking()
            .OrderByDescending(p => p.PickedAtUtc)
            .ThenBy(p => p.Number)
            .Select(p => new PickOrderSummaryDto
            {
                Id = p.Id,
                Number = p.Number,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                Reference = p.Reference,
                Status = p.Status,
                PickedAtUtc = p.PickedAtUtc,
                PlannedPickDate = p.PlannedPickDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedBy = p.CreatedBy,
                UpdatedBy = p.UpdatedBy,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee != null ? p.Employee.LastName + " " + p.Employee.FirstName : null,
                LineCount = p.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPickOrderByIdQuery(Guid Id) : IRequest<PickOrderDto?>;

public class GetPickOrderByIdQueryHandler
    : IRequestHandler<GetPickOrderByIdQuery, PickOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPickOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PickOrderDto?> Handle(
        GetPickOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PickOrders
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PickOrderDto
            {
                Id = p.Id,
                Number = p.Number,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                Reference = p.Reference,
                Status = p.Status,
                PickedAtUtc = p.PickedAtUtc,
                PlannedPickDate = p.PlannedPickDate,
                Notes = p.Notes,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee != null ? p.Employee.LastName + " " + p.Employee.FirstName : null,
                Lines = p.Lines.Select(l => new PickOrderLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    WarehouseLocationId = l.WarehouseLocationId,
                    Quantity = l.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
