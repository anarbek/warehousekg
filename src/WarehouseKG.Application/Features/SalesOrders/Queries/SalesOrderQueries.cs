using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.SalesOrders.Dtos;

namespace WarehouseKG.Application.Features.SalesOrders.Queries;

public record GetSalesOrdersQuery : IRequest<IReadOnlyList<SalesOrderSummaryDto>>;

public class GetSalesOrdersQueryHandler
    : IRequestHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SalesOrderSummaryDto>> Handle(
        GetSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SalesOrders
            .AsNoTracking()
            .OrderByDescending(s => s.OrderDateUtc)
            .ThenBy(s => s.Number)
            .Select(s => new SalesOrderSummaryDto
            {
                Id = s.Id,
                Number = s.Number,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                Status = s.Status,
                Currency = s.Currency,
                OrderDateUtc = s.OrderDateUtc,
                TotalAmount = s.Lines.Sum(l => l.Quantity * l.UnitPrice),
                LineCount = s.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetSalesOrderByIdQuery(Guid Id) : IRequest<SalesOrderDto?>;

public class GetSalesOrderByIdQueryHandler
    : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSalesOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrderDto?> Handle(
        GetSalesOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SalesOrders
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new SalesOrderDto
            {
                Id = s.Id,
                Number = s.Number,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : null,
                Status = s.Status,
                Currency = s.Currency,
                OrderDateUtc = s.OrderDateUtc,
                ExpectedDateUtc = s.ExpectedDateUtc,
                ConfirmedAtUtc = s.ConfirmedAtUtc,
                ShippedAtUtc = s.ShippedAtUtc,
                Notes = s.Notes,
                TotalAmount = s.Lines.Sum(l => l.Quantity * l.UnitPrice),
                Lines = s.Lines.Select(l => new SalesOrderLineDto
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
