using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PreOrders.Dtos;

namespace WarehouseKG.Application.Features.PreOrders.Queries;

public record GetPreOrdersQuery : IRequest<IReadOnlyList<PreOrderSummaryDto>>;

public class GetPreOrdersQueryHandler
    : IRequestHandler<GetPreOrdersQuery, IReadOnlyList<PreOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPreOrdersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<PreOrderSummaryDto>> Handle(
        GetPreOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.PreOrders
            .AsNoTracking()
            .OrderByDescending(p => p.OrderDateUtc)
            .ThenBy(p => p.Number)
            .Select(p => new PreOrderSummaryDto
            {
                Id = p.Id,
                Number = p.Number,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer != null ? p.Customer.Name : null,
                PresellerName = p.Preseller != null
                    ? p.Preseller.LastName + " " + p.Preseller.FirstName
                    : null,
                Status = p.Status,
                PaymentType = p.PaymentType,
                TotalAmount = p.TotalAmount,
                LineCount = p.Lines.Count,
                OrderDateUtc = p.OrderDateUtc,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPreOrderByIdQuery(Guid Id) : IRequest<PreOrderDto?>;

public class GetPreOrderByIdQueryHandler
    : IRequestHandler<GetPreOrderByIdQuery, PreOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPreOrderByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PreOrderDto?> Handle(
        GetPreOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.PreOrders
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PreOrderDto
            {
                Id = p.Id,
                Number = p.Number,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer != null ? p.Customer.Name : null,
                PresellerId = p.PresellerId,
                PresellerName = p.Preseller != null
                    ? p.Preseller.LastName + " " + p.Preseller.FirstName
                    : null,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                Status = p.Status,
                PaymentType = p.PaymentType,
                Currency = p.Currency,
                OrderDateUtc = p.OrderDateUtc,
                ExpectedDateUtc = p.ExpectedDateUtc,
                SubmittedAtUtc = p.SubmittedAtUtc,
                ApprovedAtUtc = p.ApprovedAtUtc,
                RejectedAtUtc = p.RejectedAtUtc,
                ConvertedAtUtc = p.ConvertedAtUtc,
                ConvertedSalesOrderId = p.ConvertedSalesOrderId,
                ConvertedSalesOrderNumber = p.ConvertedSalesOrder != null
                    ? p.ConvertedSalesOrder.Number
                    : null,
                Notes = p.Notes,
                TotalAmount = p.TotalAmount,
                Lines = p.Lines.Select(l => new PreOrderLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    InventoryItemName = l.InventoryItem != null ? l.InventoryItem.Name : null,
                    Sku = l.InventoryItem != null ? l.InventoryItem.Sku : null,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    WarehouseStockSnapshot = l.WarehouseStockSnapshot,
                    StockDifference = l.StockDifference,
                    DiscountPercent = l.DiscountPercent,
                    LineTotal = l.LineTotal,
                }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public record GetMyPreOrdersQuery : IRequest<IReadOnlyList<PreOrderSummaryDto>>;

public class GetMyPreOrdersQueryHandler
    : IRequestHandler<GetMyPreOrdersQuery, IReadOnlyList<PreOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyPreOrdersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PreOrderSummaryDto>> Handle(
        GetMyPreOrdersQuery request, CancellationToken cancellationToken)
    {
        var employeeId = _currentUser.EmployeeId;
        if (employeeId is null) return Array.Empty<PreOrderSummaryDto>();

        return await _context.PreOrders
            .AsNoTracking()
            .Where(p => p.PresellerId == employeeId.Value)
            .OrderByDescending(p => p.OrderDateUtc)
            .ThenBy(p => p.Number)
            .Select(p => new PreOrderSummaryDto
            {
                Id = p.Id,
                Number = p.Number,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer != null ? p.Customer.Name : null,
                PresellerName = p.Preseller != null
                    ? p.Preseller.LastName + " " + p.Preseller.FirstName
                    : null,
                Status = p.Status,
                PaymentType = p.PaymentType,
                TotalAmount = p.TotalAmount,
                LineCount = p.Lines.Count,
                OrderDateUtc = p.OrderDateUtc,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPaymentTypesQuery : IRequest<IReadOnlyList<PaymentTypeDto>>;

public class GetPaymentTypesQueryHandler
    : IRequestHandler<GetPaymentTypesQuery, IReadOnlyList<PaymentTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentTypesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<PaymentTypeDto>> Handle(
        GetPaymentTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.PaymentTypes
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .Select(p => new PaymentTypeDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPreOrderWarehouseStockQuery(Guid WarehouseId) : IRequest<IReadOnlyList<PreOrderWarehouseStockDto>>;

public class GetPreOrderWarehouseStockQueryHandler
    : IRequestHandler<GetPreOrderWarehouseStockQuery, IReadOnlyList<PreOrderWarehouseStockDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPreOrderWarehouseStockQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<PreOrderWarehouseStockDto>> Handle(
        GetPreOrderWarehouseStockQuery request, CancellationToken cancellationToken)
    {
        // Return all active inventory items with their current QuantityOnHand.
        // The warehouse-stock report endpoint computes per-warehouse balances by replaying operations,
        // but for pre-order purposes, the tenant-wide QuantityOnHand gives a quick stock snapshot.
        // If per-warehouse precision is needed, this can be enhanced to use the report logic.
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new PreOrderWarehouseStockDto
            {
                InventoryItemId = i.Id,
                Sku = i.Sku,
                Name = i.Name,
                QuantityOnHand = i.QuantityOnHand,
            })
            .ToListAsync(cancellationToken);
    }
}
