using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.StockAdjustments.Dtos;

namespace WarehouseKG.Application.Features.StockAdjustments.Queries;

public record GetStockAdjustmentsQuery : IRequest<IReadOnlyList<StockAdjustmentSummaryDto>>;

public class GetStockAdjustmentsQueryHandler
    : IRequestHandler<GetStockAdjustmentsQuery, IReadOnlyList<StockAdjustmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockAdjustmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockAdjustmentSummaryDto>> Handle(
        GetStockAdjustmentsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockAdjustments
            .AsNoTracking()
            .OrderByDescending(a => a.AdjustedAtUtc)
            .ThenBy(a => a.Number)
            .Select(a => new StockAdjustmentSummaryDto
            {
                Id = a.Id,
                Number = a.Number,
                WarehouseId = a.WarehouseId,
                Reason = a.Reason,
                Status = a.Status,
                AdjustedAtUtc = a.AdjustedAtUtc,
                LineCount = a.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetStockAdjustmentByIdQuery(Guid Id) : IRequest<StockAdjustmentDto?>;

public class GetStockAdjustmentByIdQueryHandler
    : IRequestHandler<GetStockAdjustmentByIdQuery, StockAdjustmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStockAdjustmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockAdjustmentDto?> Handle(
        GetStockAdjustmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockAdjustments
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new StockAdjustmentDto
            {
                Id = a.Id,
                Number = a.Number,
                WarehouseId = a.WarehouseId,
                Reason = a.Reason,
                Status = a.Status,
                AdjustedAtUtc = a.AdjustedAtUtc,
                Notes = a.Notes,
                Lines = a.Lines.Select(l => new StockAdjustmentLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    QuantityChange = l.QuantityChange,
                    Notes = l.Notes
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
