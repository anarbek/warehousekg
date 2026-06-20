using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Dispatching.Routes.Queries;

namespace WarehouseKG.Application.Features.Dispatching.Stops.Queries;

public record GetStopsByRouteQuery(Guid RouteId) : IRequest<IReadOnlyList<StopDto>>;

public class GetStopsByRouteQueryHandler : IRequestHandler<GetStopsByRouteQuery, IReadOnlyList<StopDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStopsByRouteQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<StopDto>> Handle(GetStopsByRouteQuery request, CancellationToken ct)
    {
        return await _context.DeliveryStops
            .AsNoTracking()
            .Where(s => s.RouteId == request.RouteId)
            .OrderBy(s => s.SequenceNumber)
            .Select(s => new StopDto
            {
                Id = s.Id,
                RouteId = s.RouteId,
                SequenceNumber = s.SequenceNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                PlannedArrivalUtc = s.PlannedArrivalUtc,
                PlannedDepartureUtc = s.PlannedDepartureUtc,
                ActualArrivalUtc = s.ActualArrivalUtc,
                ActualDepartureUtc = s.ActualDepartureUtc,
                Status = s.Status,
                HasRegulatedGoods = s.HasRegulatedGoods,
                Notes = s.Notes,
                ShipmentCount = s.Shipments.Count
            })
            .ToListAsync(ct);
    }
}

public record GetDeliveryStopByIdQuery(Guid Id) : IRequest<StopDetailDto?>;

public class GetDeliveryStopByIdQueryHandler : IRequestHandler<GetDeliveryStopByIdQuery, StopDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDeliveryStopByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<StopDetailDto?> Handle(GetDeliveryStopByIdQuery request, CancellationToken ct)
    {
        return await _context.DeliveryStops
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new StopDetailDto
            {
                Id = s.Id,
                RouteId = s.RouteId,
                SequenceNumber = s.SequenceNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                PlannedArrivalUtc = s.PlannedArrivalUtc,
                PlannedDepartureUtc = s.PlannedDepartureUtc,
                ActualArrivalUtc = s.ActualArrivalUtc,
                ActualDepartureUtc = s.ActualDepartureUtc,
                Status = s.Status,
                HasRegulatedGoods = s.HasRegulatedGoods,
                Notes = s.Notes,
                ShipmentCount = s.Shipments.Count,
                Shipments = s.Shipments
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
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
    }
}
