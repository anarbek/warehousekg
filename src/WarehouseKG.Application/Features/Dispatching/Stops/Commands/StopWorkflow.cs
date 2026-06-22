using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Stops.Commands;

public record ArriveAtStopCommand(Guid Id) : IRequest<OperationResult>;

public class ArriveAtStopCommandHandler : IRequestHandler<ArriveAtStopCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public ArriveAtStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(ArriveAtStopCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (stop is null) return OperationResult.NotFound;
        if (stop.Status != StopStatus.Pending) return OperationResult.InvalidState;
        if (stop.Route is not null && stop.Route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            return OperationResult.InvalidState;

        stop.Status = StopStatus.InProgress;
        stop.ActualArrivalUtc = DateTime.UtcNow;

        // Check geofence containment: log alerts for restricted/no-go zones
        if (stop.Latitude.HasValue && stop.Longitude.HasValue)
        {
            var stopPoint = new WarehouseKG.Domain.Common.GeoPoint(stop.Latitude.Value, stop.Longitude.Value);

            var restrictedGeofences = await _context.Geofences
                .Where(g => g.IsActive
                    && (g.Type == GeofenceType.Restricted || g.Type == GeofenceType.NoGo))
                .ToListAsync(ct);

            foreach (var geofence in restrictedGeofences)
            {
                if (geofence.Vertices.Count >= 3 && GeoUtils.IsPointInPolygon(stopPoint, geofence.Vertices))
                {
                    stop.Notes = (stop.Notes ?? "") +
                        $"\n[GEOFENCE] Entered {geofence.Type} zone '{geofence.Name}' at {DateTime.UtcNow:O}";
                }
            }
        }

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}

public record CompleteDeliveryStopCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteDeliveryStopCommandHandler : IRequestHandler<CompleteDeliveryStopCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteDeliveryStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(CompleteDeliveryStopCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Shipments)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (stop is null) return OperationResult.NotFound;
        if (stop.Status != StopStatus.InProgress) return OperationResult.InvalidState;
        if (stop.Route is not null && stop.Route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            return OperationResult.InvalidState;

        stop.Status = StopStatus.Completed;
        stop.ActualDepartureUtc = DateTime.UtcNow;

        // Mark all shipments as completed
        foreach (var shipment in stop.Shipments)
        {
            shipment.Status = StopStatus.Completed;
        }

        await _context.SaveChangesAsync(ct);

        // Auto-transition sales orders to Shipped when all their shipments are completed
        var salesOrderIds = stop.Shipments.Select(sh => sh.SalesOrderId).Distinct().ToList();
        if (salesOrderIds.Count > 0)
        {
            var allCompletedIds = await _context.DeliveryShipments
                .Where(sh => salesOrderIds.Contains(sh.SalesOrderId))
                .GroupBy(sh => sh.SalesOrderId)
                .Where(g => g.All(sh => sh.Status == StopStatus.Completed))
                .Select(g => g.Key)
                .ToListAsync(ct);

            if (allCompletedIds.Count > 0)
            {
                var orders = await _context.SalesOrders
                    .Include(so => so.Lines)
                    .Where(so => allCompletedIds.Contains(so.Id) && so.Status == SalesOrderStatus.Confirmed)
                    .ToListAsync(ct);

                if (orders.Count > 0)
                {
                    // Deduct inventory for all shipped orders
                    var itemIds = orders.SelectMany(o => o.Lines).Select(l => l.InventoryItemId).Distinct().ToList();
                    var items = await _context.InventoryItems
                        .Where(i => itemIds.Contains(i.Id))
                        .ToListAsync(ct);

                    foreach (var order in orders)
                    {
                        order.Status = SalesOrderStatus.Shipped;
                        order.ShippedAtUtc = DateTime.UtcNow;

                        foreach (var line in order.Lines)
                        {
                            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
                            if (item is not null)
                            {
                                item.QuantityOnHand -= line.Quantity;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync(ct);

                // Auto-create invoices for shipped sales orders
                foreach (var order in orders)
                {
                    var invoice = new Invoice
                    {
                        Id = Guid.NewGuid(),
                        Type = InvoiceType.Sales,
                        Status = InvoiceStatus.Issued,
                        SalesOrderId = order.Id,
                        CustomerId = order.CustomerId,
                        WarehouseId = order.WarehouseId ?? Guid.Empty,
                        Currency = order.Currency,
                        IssuedAtUtc = DateTime.UtcNow,
                        Notes = $"Auto-generated from SalesOrder {order.Number}",
                        Lines = order.Lines.Select(l => new InvoiceLine
                        {
                            Id = Guid.NewGuid(),
                            InventoryItemId = l.InventoryItemId,
                            Quantity = l.Quantity,
                            UnitPrice = l.UnitPrice,
                            LineTotal = l.Quantity * l.UnitPrice,
                            TaxRate = 0m,
                            TaxAmount = 0m
                        }).ToList()
                    };

                    // Generate invoice number
                    var year = DateTime.UtcNow.Year;
                    var prefix = $"INV-{year}-";
                    var lastInvoice = await _context.Invoices
                        .Where(i => i.Number.StartsWith(prefix))
                        .OrderByDescending(i => i.Number)
                        .FirstOrDefaultAsync(ct);
                    if (lastInvoice is not null && int.TryParse(lastInvoice.Number[prefix.Length..], out var num))
                        invoice.Number = $"{prefix}{(num + 1):D4}";
                    else
                        invoice.Number = $"{prefix}0001";

                    invoice.TotalAmount = invoice.Lines.Sum(l => l.LineTotal);
                    invoice.TaxAmount = invoice.Lines.Sum(l => l.TaxAmount);

                    _context.Invoices.Add(invoice);
                }

                await _context.SaveChangesAsync(ct);
            }
        }

        return OperationResult.Success;
    }
}

public record SkipDeliveryStopCommand(Guid Id) : IRequest<OperationResult>;

public class SkipDeliveryStopCommandHandler : IRequestHandler<SkipDeliveryStopCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public SkipDeliveryStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(SkipDeliveryStopCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Shipments)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (stop is null) return OperationResult.NotFound;
        if (stop.Status is StopStatus.Completed) return OperationResult.InvalidState;
        if (stop.Route is not null && stop.Route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            return OperationResult.InvalidState;

        stop.Status = StopStatus.Skipped;

        // Mark all shipments as skipped
        foreach (var shipment in stop.Shipments)
        {
            shipment.Status = StopStatus.Skipped;
        }

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}
