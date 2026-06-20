using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Routes.Commands;

public record StartDeliveryRouteCommand(Guid Id) : IRequest<OperationResult>;

public class StartDeliveryRouteCommandHandler : IRequestHandler<StartDeliveryRouteCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public StartDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(StartDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (route is null) return OperationResult.NotFound;
        if (route.Status != RouteStatus.Planned) return OperationResult.InvalidState;

        route.Status = RouteStatus.InProgress;

        // Set all pending stops that haven't started yet — they remain Pending.
        // The driver will arrive/complete each stop individually.

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}

public record CompleteDeliveryRouteCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteDeliveryRouteCommandHandler : IRequestHandler<CompleteDeliveryRouteCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(CompleteDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (route is null) return OperationResult.NotFound;
        if (route.Status != RouteStatus.InProgress) return OperationResult.InvalidState;

        // Auto-skip any stops still pending
        foreach (var stop in route.Stops)
        {
            if (stop.Status == StopStatus.Pending)
            {
                stop.Status = StopStatus.Skipped;
            }
        }

        route.Status = RouteStatus.Completed;
        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}

public record CancelDeliveryRouteCommand(Guid Id) : IRequest<OperationResult>;

public class CancelDeliveryRouteCommandHandler : IRequestHandler<CancelDeliveryRouteCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelDeliveryRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(CancelDeliveryRouteCommand request, CancellationToken ct)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (route is null) return OperationResult.NotFound;
        if (route.Status is RouteStatus.Completed) return OperationResult.InvalidState;

        route.Status = RouteStatus.Cancelled;
        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}
