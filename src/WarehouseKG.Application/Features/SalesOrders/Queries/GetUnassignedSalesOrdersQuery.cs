using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.SalesOrders.Queries;

/// <summary>
/// Returns confirmed sales orders that are NOT yet assigned to any delivery shipment.
/// </summary>
public record GetConfirmedUnassignedSalesOrdersQuery : IRequest<IReadOnlyList<UnassignedSalesOrderDto>>;

public class GetConfirmedUnassignedSalesOrdersQueryHandler
    : IRequestHandler<GetConfirmedUnassignedSalesOrdersQuery, IReadOnlyList<UnassignedSalesOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetConfirmedUnassignedSalesOrdersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<UnassignedSalesOrderDto>> Handle(
        GetConfirmedUnassignedSalesOrdersQuery request, CancellationToken ct)
    {
        var assignedIds = await _context.DeliveryShipments
            .AsNoTracking()
            .Select(sh => sh.SalesOrderId)
            .Distinct()
            .ToListAsync(ct);

        return await _context.SalesOrders
            .AsNoTracking()
            .Where(so => so.Status == SalesOrderStatus.Confirmed && !assignedIds.Contains(so.Id))
            .OrderBy(so => so.Number)
            .Select(so => new UnassignedSalesOrderDto
            {
                Id = so.Id,
                Number = so.Number,
                CustomerName = so.Customer != null ? so.Customer.Name : null,
                TotalAmount = so.Lines.Sum(l => l.Quantity * l.UnitPrice),
                LineCount = so.Lines.Count
            })
            .ToListAsync(ct);
    }
}

public class UnassignedSalesOrderDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
}
