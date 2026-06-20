using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Routes.Commands;

public record CreateDeliveryRouteCommand(
    string Code,
    DateTime Date,
    Guid? VehicleId = null,
    Guid? DriverEmployeeId = null,
    string? Notes = null) : IRequest<Guid>;

public class CreateDeliveryRouteCommandHandler : IRequestHandler<CreateDeliveryRouteCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = new DeliveryRoute
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Date = request.Date,
            VehicleId = request.VehicleId,
            DriverEmployeeId = request.DriverEmployeeId,
            Notes = request.Notes,
            Status = RouteStatus.Planned
        };

        _context.DeliveryRoutes.Add(route);
        await _context.SaveChangesAsync(ct);
        return route.Id;
    }
}

public record UpdateDeliveryRouteCommand(
    Guid Id,
    string Code,
    DateTime Date,
    Guid? VehicleId,
    Guid? DriverEmployeeId,
    string? Notes) : IRequest<bool>;

public class UpdateDeliveryRouteCommandHandler : IRequestHandler<UpdateDeliveryRouteCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes.FindAsync(new object[] { request.Id }, ct);
        if (route is null) return false;
        if (route.Status is RouteStatus.Completed or RouteStatus.Cancelled) return false;

        route.Code = request.Code;
        route.Date = request.Date;
        route.VehicleId = request.VehicleId;
        route.DriverEmployeeId = request.DriverEmployeeId;
        route.Notes = request.Notes;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

public record DeleteDeliveryRouteCommand(Guid Id) : IRequest<bool>;

public class DeleteDeliveryRouteCommandHandler : IRequestHandler<DeleteDeliveryRouteCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes.FindAsync(new object[] { request.Id }, ct);
        if (route is null) return false;

        _context.DeliveryRoutes.Remove(route);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
