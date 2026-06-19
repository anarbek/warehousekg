using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.StockAudits.Dtos;

namespace WarehouseKG.Application.Features.StockAudits.Queries;

public record GetStockAuditsQuery : IRequest<IReadOnlyList<StockAuditSummaryDto>>;

public class GetStockAuditsQueryHandler
    : IRequestHandler<GetStockAuditsQuery, IReadOnlyList<StockAuditSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockAuditsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockAuditSummaryDto>> Handle(
        GetStockAuditsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockAudits
            .AsNoTracking()
            .OrderByDescending(a => a.ReconciledAtUtc)
            .ThenBy(a => a.Number)
            .Select(a => new StockAuditSummaryDto
            {
                Id = a.Id,
                Number = a.Number,
                WarehouseId = a.WarehouseId,
                WarehouseName = a.Warehouse != null ? a.Warehouse.Name : null,
                Status = a.Status,
                ReconciledAtUtc = a.ReconciledAtUtc,
                CreatedAt = a.CreatedAt,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null,
                LineCount = a.Lines.Count,
                TotalVariance = a.Lines.Sum(l => l.CountedQuantity - l.SystemQuantity)
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetStockAuditByIdQuery(Guid Id) : IRequest<StockAuditDto?>;

public class GetStockAuditByIdQueryHandler
    : IRequestHandler<GetStockAuditByIdQuery, StockAuditDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStockAuditByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockAuditDto?> Handle(
        GetStockAuditByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StockAudits
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new StockAuditDto
            {
                Id = a.Id,
                Number = a.Number,
                WarehouseId = a.WarehouseId,
                WarehouseName = a.Warehouse != null ? a.Warehouse.Name : null,
                Status = a.Status,
                ReconciledAtUtc = a.ReconciledAtUtc,
                Notes = a.Notes,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null,
                Lines = a.Lines.Select(l => new StockAuditLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    InventoryItemName = l.InventoryItem != null ? l.InventoryItem.Name : null,
                    InventoryItemSku = l.InventoryItem != null ? l.InventoryItem.Sku : null,
                    SystemQuantity = l.SystemQuantity,
                    CountedQuantity = l.CountedQuantity,
                    Variance = l.CountedQuantity - l.SystemQuantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
