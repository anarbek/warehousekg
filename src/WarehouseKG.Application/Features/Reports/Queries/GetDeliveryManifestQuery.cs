using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;

namespace WarehouseKG.Application.Features.Reports.Queries;

public record GetDeliveryManifestQuery(Guid RouteId) : IRequest<DeliveryManifestDto?>;

public class GetDeliveryManifestQueryHandler : IRequestHandler<GetDeliveryManifestQuery, DeliveryManifestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDeliveryManifestQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DeliveryManifestDto?> Handle(GetDeliveryManifestQuery request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes
            .AsNoTracking()
            .Include(r => r.Vehicle)
            .Include(r => r.DriverEmployee)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Customer)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Shipments)
                    .ThenInclude(sh => sh.SalesOrder)
                        .ThenInclude(so => so != null ? so.Lines : null)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Shipments)
                    .ThenInclude(sh => sh.SalesOrder)
                        .ThenInclude(so => so != null ? so.Customer : null)
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, ct);

        if (route is null) return null;

        return new DeliveryManifestDto
        {
            RouteId = route.Id,
            RouteCode = route.Code,
            Date = route.Date,
            VehicleCode = route.Vehicle?.Code,
            DriverName = route.DriverEmployee != null
                ? $"{route.DriverEmployee.LastName} {route.DriverEmployee.FirstName}"
                : null,
            Status = route.Status.ToString(),
            Stops = route.Stops
                .OrderBy(s => s.SequenceNumber)
                .Select(s => new ManifestStopDto
                {
                    SequenceNumber = s.SequenceNumber,
                    CustomerName = s.Customer?.Name,
                    Address = s.Address,
                    Status = s.Status.ToString(),
                    Shipments = s.Shipments
                        .Select(sh => new ManifestShipmentDto
                        {
                            OrderNumber = sh.SalesOrder?.Number ?? "",
                            Lines = sh.SalesOrder?.Lines
                                .Select(l => new ManifestLineDto
                                {
                                    ItemName = l.InventoryItem?.Name ?? "",
                                    ItemSku = l.InventoryItem?.Sku ?? "",
                                    Quantity = l.Quantity
                                }).ToList() ?? new()
                        }).ToList()
                }).ToList()
        };
    }
}
