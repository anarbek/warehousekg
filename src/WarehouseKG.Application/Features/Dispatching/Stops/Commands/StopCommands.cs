using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Stops.Commands;

public record CreateDeliveryStopCommand(
    Guid RouteId,
    int SequenceNumber,
    Guid? CustomerId,
    string Address,
    double? Latitude = null,
    double? Longitude = null,
    DateTime? PlannedArrivalUtc = null,
    DateTime? PlannedDepartureUtc = null,
    string? Notes = null) : IRequest<Guid>;

public class CreateDeliveryStopCommandHandler : IRequestHandler<CreateDeliveryStopCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDeliveryStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateDeliveryStopCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes.FindAsync(new object[] { request.RouteId }, ct);
        if (route is null) throw new InvalidOperationException("Route not found");
        if (route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a completed or cancelled route");

        var stop = new DeliveryStop
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            SequenceNumber = request.SequenceNumber,
            CustomerId = request.CustomerId,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PlannedArrivalUtc = request.PlannedArrivalUtc,
            PlannedDepartureUtc = request.PlannedDepartureUtc,
            Notes = request.Notes,
            Status = StopStatus.Pending,
            HasRegulatedGoods = false
        };

        _context.DeliveryStops.Add(stop);
        await _context.SaveChangesAsync(ct);
        return stop.Id;
    }
}

public record UpdateDeliveryStopCommand(
    Guid Id,
    int SequenceNumber,
    Guid? CustomerId,
    string Address,
    double? Latitude,
    double? Longitude,
    DateTime? PlannedArrivalUtc,
    DateTime? PlannedDepartureUtc,
    string? Notes) : IRequest<bool>;

public class UpdateDeliveryStopCommandHandler : IRequestHandler<UpdateDeliveryStopCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateDeliveryStopCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);
        if (stop is null) return false;
        if (stop.Route is not null && stop.Route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            return false;

        stop.SequenceNumber = request.SequenceNumber;
        stop.CustomerId = request.CustomerId;
        stop.Address = request.Address;
        stop.Latitude = request.Latitude;
        stop.Longitude = request.Longitude;
        stop.PlannedArrivalUtc = request.PlannedArrivalUtc;
        stop.PlannedDepartureUtc = request.PlannedDepartureUtc;
        stop.Notes = request.Notes;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

public record DeleteDeliveryStopCommand(Guid Id) : IRequest<bool>;

public class DeleteDeliveryStopCommandHandler : IRequestHandler<DeleteDeliveryStopCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDeliveryStopCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteDeliveryStopCommand request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);
        if (stop is null) return false;
        if (stop.Route is not null && stop.Route.Status is RouteStatus.Completed or RouteStatus.Cancelled)
            return false;

        _context.DeliveryStops.Remove(stop);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
