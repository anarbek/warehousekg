using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.StockTransfers.Dtos;

namespace WarehouseKG.Application.Features.StockTransfers.Queries;

public record GetStockTransfersQuery : IRequest<IReadOnlyList<StockTransferSummaryDto>>;

public class GetStockTransfersQueryHandler
    : IRequestHandler<GetStockTransfersQuery, IReadOnlyList<StockTransferSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockTransfersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockTransferSummaryDto>> Handle(
        GetStockTransfersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .OrderByDescending(t => t.TransferredAtUtc)
            .ThenBy(t => t.Number)
            .Select(t => new StockTransferSummaryDto
            {
                Id = t.Id,
                Number = t.Number,
                SourceWarehouseId = t.SourceWarehouseId,
                SourceWarehouseName = t.SourceWarehouse != null ? t.SourceWarehouse.Name : null,
                DestinationWarehouseId = t.DestinationWarehouseId,
                DestinationWarehouseName = t.DestinationWarehouse != null ? t.DestinationWarehouse.Name : null,
                Status = t.Status,
                TransferredAtUtc = t.TransferredAtUtc,
                LineCount = t.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetStockTransferByIdQuery(Guid Id) : IRequest<StockTransferDto?>;

public class GetStockTransferByIdQueryHandler
    : IRequestHandler<GetStockTransferByIdQuery, StockTransferDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStockTransferByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockTransferDto?> Handle(
        GetStockTransferByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new StockTransferDto
            {
                Id = t.Id,
                Number = t.Number,
                SourceWarehouseId = t.SourceWarehouseId,
                SourceWarehouseName = t.SourceWarehouse != null ? t.SourceWarehouse.Name : null,
                DestinationWarehouseId = t.DestinationWarehouseId,
                DestinationWarehouseName = t.DestinationWarehouse != null ? t.DestinationWarehouse.Name : null,
                Status = t.Status,
                TransferredAtUtc = t.TransferredAtUtc,
                Notes = t.Notes,
                Lines = t.Lines.Select(l => new StockTransferLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
