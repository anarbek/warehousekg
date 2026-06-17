using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.StockReceipts.Dtos;

namespace WarehouseKG.Application.Features.StockReceipts.Queries;

public record GetStockReceiptsQuery : IRequest<IReadOnlyList<StockReceiptSummaryDto>>;

public class GetStockReceiptsQueryHandler
    : IRequestHandler<GetStockReceiptsQuery, IReadOnlyList<StockReceiptSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockReceiptsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockReceiptSummaryDto>> Handle(
        GetStockReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockReceipts
            .AsNoTracking()
            .OrderByDescending(r => r.ReceivedAtUtc)
            .ThenBy(r => r.Number)
            .Select(r => new StockReceiptSummaryDto
            {
                Id = r.Id,
                Number = r.Number,
                WarehouseId = r.WarehouseId,
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : null,
                SupplierReference = r.SupplierReference,
                Status = r.Status,
                ReceivedAtUtc = r.ReceivedAtUtc,
                TransactionDate = r.TransactionDate,
                LineCount = r.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetStockReceiptByIdQuery(Guid Id) : IRequest<StockReceiptDto?>;

public class GetStockReceiptByIdQueryHandler
    : IRequestHandler<GetStockReceiptByIdQuery, StockReceiptDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStockReceiptByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockReceiptDto?> Handle(
        GetStockReceiptByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockReceipts
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new StockReceiptDto
            {
                Id = r.Id,
                Number = r.Number,
                WarehouseId = r.WarehouseId,
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : null,
                SupplierReference = r.SupplierReference,
                Status = r.Status,
                ReceivedAtUtc = r.ReceivedAtUtc,
                TransactionDate = r.TransactionDate,
                Notes = r.Notes,
                Lines = r.Lines.Select(l => new StockReceiptLineDto
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
