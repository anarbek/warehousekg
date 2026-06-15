using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;

namespace WarehouseKG.Application.Features.Reports.Queries;

public record GetSalesSummaryReportQuery : IRequest<SalesSummaryReportDto>;

public class GetSalesSummaryReportQueryHandler
    : IRequestHandler<GetSalesSummaryReportQuery, SalesSummaryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalesSummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesSummaryReportDto> Handle(
        GetSalesSummaryReportQuery request,
        CancellationToken cancellationToken)
    {
        // Project each order to its status + computed amount in the database, then aggregate
        // in memory. This keeps the correlated line-sum translatable while avoiding a GroupBy
        // over a nested aggregate, which providers struggle to translate.
        var orders = await _context.SalesOrders
            .AsNoTracking()
            .Select(o => new
            {
                o.Status,
                Amount = o.Lines.Sum(l => l.Quantity * l.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        var byStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusBreakdownDto
            {
                Status = g.Key.ToString(),
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.Amount)
            })
            .OrderBy(s => s.Status)
            .ToList();

        return new SalesSummaryReportDto
        {
            TotalOrders = orders.Count,
            TotalAmount = orders.Sum(o => o.Amount),
            ByStatus = byStatus
        };
    }
}

public record GetPurchaseSummaryReportQuery : IRequest<PurchaseSummaryReportDto>;

public class GetPurchaseSummaryReportQueryHandler
    : IRequestHandler<GetPurchaseSummaryReportQuery, PurchaseSummaryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseSummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseSummaryReportDto> Handle(
        GetPurchaseSummaryReportQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .Select(o => new
            {
                o.Status,
                Amount = o.Lines.Sum(l => l.Quantity * l.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        var byStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusBreakdownDto
            {
                Status = g.Key.ToString(),
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.Amount)
            })
            .OrderBy(s => s.Status)
            .ToList();

        return new PurchaseSummaryReportDto
        {
            TotalOrders = orders.Count,
            TotalAmount = orders.Sum(o => o.Amount),
            ByStatus = byStatus
        };
    }
}
