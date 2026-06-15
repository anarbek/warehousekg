using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Reports.Queries;

public record GetStockMovementSummaryReportQuery : IRequest<StockMovementSummaryReportDto>;

public class GetStockMovementSummaryReportQueryHandler
    : IRequestHandler<GetStockMovementSummaryReportQuery, StockMovementSummaryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetStockMovementSummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovementSummaryReportDto> Handle(
        GetStockMovementSummaryReportQuery request,
        CancellationToken cancellationToken)
    {
        return new StockMovementSummaryReportDto
        {
            Operations = new List<OperationStatusCountsDto>
            {
                await SummarizeAsync(_context.StockReceipts, "Receipts", r => r.Status, cancellationToken),
                await SummarizeAsync(_context.PickOrders, "Picks", p => p.Status, cancellationToken),
                await SummarizeAsync(_context.PackOrders, "Packs", p => p.Status, cancellationToken),
                await SummarizeAsync(_context.StockTransfers, "Transfers", t => t.Status, cancellationToken),
                await SummarizeAsync(_context.StockAdjustments, "Adjustments", a => a.Status, cancellationToken),
                await SummarizeAsync(_context.StockAudits, "Audits", a => a.Status, cancellationToken)
            }
        };
    }

    private static async Task<OperationStatusCountsDto> SummarizeAsync<T>(
        IQueryable<T> source,
        string operation,
        Expression<Func<T, StockOperationStatus>> statusSelector,
        CancellationToken cancellationToken)
        where T : class
    {
        var counts = await source
            .AsNoTracking()
            .GroupBy(statusSelector)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var dto = new OperationStatusCountsDto { Operation = operation };

        foreach (var entry in counts)
        {
            switch (entry.Status)
            {
                case StockOperationStatus.Draft:
                    dto.Draft = entry.Count;
                    break;
                case StockOperationStatus.Completed:
                    dto.Completed = entry.Count;
                    break;
                case StockOperationStatus.Cancelled:
                    dto.Cancelled = entry.Count;
                    break;
            }
        }

        dto.Total = dto.Draft + dto.Completed + dto.Cancelled;
        return dto;
    }
}
