using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Dispatching.Stops.Queries;
using ShipmentDto = WarehouseKG.Application.Features.Dispatching.Routes.Queries.ShipmentDto;

namespace WarehouseKG.Application.Features.Dispatching.Shipments.Queries;

public record GetShipmentsByStopQuery(Guid StopId) : IRequest<IReadOnlyList<ShipmentDto>>;

public class GetShipmentsByStopQueryHandler : IRequestHandler<GetShipmentsByStopQuery, IReadOnlyList<ShipmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShipmentsByStopQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ShipmentDto>> Handle(GetShipmentsByStopQuery request, CancellationToken ct)
    {
        return await _context.DeliveryShipments
            .AsNoTracking()
            .Where(sh => sh.DeliveryStopId == request.StopId)
            .OrderBy(sh => sh.SalesOrder != null ? sh.SalesOrder.Number : "")
            .Select(sh => new ShipmentDto
            {
                Id = sh.Id,
                DeliveryStopId = sh.DeliveryStopId,
                SalesOrderId = sh.SalesOrderId,
                SalesOrderNumber = sh.SalesOrder != null ? sh.SalesOrder.Number : null,
                CustomerName = sh.SalesOrder != null && sh.SalesOrder.Customer != null
                    ? sh.SalesOrder.Customer.Name : null,
                Status = sh.Status
            })
            .ToListAsync(ct);
    }
}
