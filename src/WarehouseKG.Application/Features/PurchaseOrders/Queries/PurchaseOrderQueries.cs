using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PurchaseOrders.Dtos;

namespace WarehouseKG.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrdersQuery : IRequest<IReadOnlyList<PurchaseOrderSummaryDto>>;

public class GetPurchaseOrdersQueryHandler
    : IRequestHandler<GetPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PurchaseOrderSummaryDto>> Handle(
        GetPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(p => p.OrderDateUtc)
            .ThenBy(p => p.Number)
            .Select(p => new PurchaseOrderSummaryDto
            {
                Id = p.Id,
                Number = p.Number,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                Status = p.Status,
                Currency = p.Currency,
                OrderDateUtc = p.OrderDateUtc,
                CreatedAt = p.CreatedAt,
                ReceivedAtUtc = p.ReceivedAtUtc,
                TotalAmount = p.Lines.Sum(l => l.Quantity * l.UnitPrice),
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee != null ? p.Employee.LastName + " " + p.Employee.FirstName : null,
                LineCount = p.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<PurchaseOrderDto?>;

public class GetPurchaseOrderByIdQueryHandler
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrderDto?> Handle(
        GetPurchaseOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PurchaseOrderDto
            {
                Id = p.Id,
                Number = p.Number,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                Status = p.Status,
                Currency = p.Currency,
                OrderDateUtc = p.OrderDateUtc,
                SubmittedAtUtc = p.SubmittedAtUtc,
                ReceivedAtUtc = p.ReceivedAtUtc,
                Notes = p.Notes,
                TotalAmount = p.Lines.Sum(l => l.Quantity * l.UnitPrice),
                Lines = p.Lines.Select(l => new PurchaseOrderLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.Quantity * l.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
