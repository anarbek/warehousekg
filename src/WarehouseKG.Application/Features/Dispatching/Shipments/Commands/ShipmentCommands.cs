using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Dispatching.Shipments.Commands;

public record AssignShipmentCommand(
    Guid DeliveryStopId,
    Guid SalesOrderId) : IRequest<OperationResult>;

public class AssignShipmentCommandHandler : IRequestHandler<AssignShipmentCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public AssignShipmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(AssignShipmentCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.DeliveryStopId, ct);

        if (stop is null) return OperationResult.NotFound;
        if (stop.Route is not null && stop.Route.Status is Domain.Enums.RouteStatus.Completed or Domain.Enums.RouteStatus.Cancelled)
            return OperationResult.InvalidState;

        var salesOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(so => so.Id == request.SalesOrderId, ct);

        if (salesOrder is null) return OperationResult.NotFound;

        // Only confirmed orders can be assigned for delivery
        if (salesOrder.Status != Domain.Enums.SalesOrderStatus.Confirmed)
            return OperationResult.InvalidState;

        var existing = await _context.DeliveryShipments
            .AnyAsync(sh => sh.SalesOrderId == request.SalesOrderId, ct);

        if (existing) return OperationResult.InvalidState;

        var shipment = new DeliveryShipment
        {
            Id = Guid.NewGuid(),
            DeliveryStopId = request.DeliveryStopId,
            SalesOrderId = request.SalesOrderId,
            Status = Domain.Enums.StopStatus.Pending
        };

        _context.DeliveryShipments.Add(shipment);

        // Recalculate HasRegulatedGoods for the stop:
        // check if any linked sales order has lines with items from
        // categories marked RequiresAgeVerification.
        await RecalculateRegulatedFlag(_context, stop, shipment.SalesOrderId, ct);

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }

    private static async Task RecalculateRegulatedFlag(
        IApplicationDbContext context,
        DeliveryStop stop,
        Guid? newSalesOrderId,
        CancellationToken ct)
    {
        // Collect all sales order IDs linked to this stop (including the new one)
        var shipmentSalesOrderIds = await context.DeliveryShipments
            .Where(sh => sh.DeliveryStopId == stop.Id)
            .Select(sh => sh.SalesOrderId)
            .ToListAsync(ct);

        if (newSalesOrderId.HasValue)
            shipmentSalesOrderIds.Add(newSalesOrderId.Value);

        if (shipmentSalesOrderIds.Count == 0)
        {
            stop.HasRegulatedGoods = false;
            return;
        }

        var hasRegulated = await context.SalesOrderLines
            .Where(line => shipmentSalesOrderIds.Contains(line.SalesOrderId))
            .AnyAsync(line =>
                line.InventoryItem != null &&
                line.InventoryItem.Category != null &&
                line.InventoryItem.Category.RequiresAgeVerification,
                ct);

        stop.HasRegulatedGoods = hasRegulated;
    }
}

public record RemoveShipmentCommand(Guid ShipmentId) : IRequest<OperationResult>;

public class RemoveShipmentCommandHandler : IRequestHandler<RemoveShipmentCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public RemoveShipmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(RemoveShipmentCommand request, CancellationToken ct)
    {
        var shipment = await _context.DeliveryShipments
            .Include(sh => sh.DeliveryStop)
            .ThenInclude(s => s!.Route)
            .FirstOrDefaultAsync(sh => sh.Id == request.ShipmentId, ct);

        if (shipment is null) return OperationResult.NotFound;
        if (shipment.DeliveryStop?.Route is not null
            && shipment.DeliveryStop.Route.Status is Domain.Enums.RouteStatus.Completed or Domain.Enums.RouteStatus.Cancelled)
            return OperationResult.InvalidState;

        var stopId = shipment.DeliveryStopId;

        _context.DeliveryShipments.Remove(shipment);

        // Recalculate HasRegulatedGoods for the stop
        var stop = await _context.DeliveryStops
            .FirstOrDefaultAsync(s => s.Id == stopId, ct);

        if (stop is not null)
        {
            var remainingSalesOrderIds = await _context.DeliveryShipments
                .Where(sh => sh.DeliveryStopId == stopId)
                .Where(sh => sh.Id != request.ShipmentId)
                .Select(sh => sh.SalesOrderId)
                .ToListAsync(ct);

            stop.HasRegulatedGoods = remainingSalesOrderIds.Count > 0 &&
                await _context.SalesOrderLines
                    .Where(line => remainingSalesOrderIds.Contains(line.SalesOrderId))
                    .AnyAsync(line =>
                        line.InventoryItem != null &&
                        line.InventoryItem.Category != null &&
                        line.InventoryItem.Category.RequiresAgeVerification,
                        ct);
        }

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}
