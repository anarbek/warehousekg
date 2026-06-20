using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Geofences.Commands;

public record GeoPointInput(double Latitude, double Longitude);

public record CreateGeofenceCommand(
    string Code,
    string Name,
    GeofenceType Type,
    List<GeoPointInput> Vertices,
    bool IsActive = true) : IRequest<Guid>;

public class CreateGeofenceCommandHandler : IRequestHandler<CreateGeofenceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGeofenceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateGeofenceCommand request, CancellationToken ct)
    {
        var geofence = new Geofence
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Vertices = request.Vertices.Select(v => new GeoPoint(v.Latitude, v.Longitude)).ToList(),
            IsActive = request.IsActive
        };

        _context.Geofences.Add(geofence);
        await _context.SaveChangesAsync(ct);
        return geofence.Id;
    }
}

public record UpdateGeofenceCommand(
    Guid Id,
    string Code,
    string Name,
    GeofenceType Type,
    List<GeoPointInput> Vertices,
    bool IsActive) : IRequest<bool>;

public class UpdateGeofenceCommandHandler : IRequestHandler<UpdateGeofenceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateGeofenceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateGeofenceCommand request, CancellationToken ct)
    {
        var geofence = await _context.Geofences.FindAsync(new object[] { request.Id }, ct);
        if (geofence is null) return false;

        geofence.Code = request.Code;
        geofence.Name = request.Name;
        geofence.Type = request.Type;
        geofence.Vertices = request.Vertices.Select(v => new GeoPoint(v.Latitude, v.Longitude)).ToList();
        geofence.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

public record DeleteGeofenceCommand(Guid Id) : IRequest<bool>;

public class DeleteGeofenceCommandHandler : IRequestHandler<DeleteGeofenceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteGeofenceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteGeofenceCommand request, CancellationToken ct)
    {
        var geofence = await _context.Geofences.FindAsync(new object[] { request.Id }, ct);
        if (geofence is null) return false;

        _context.Geofences.Remove(geofence);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
