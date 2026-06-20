using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Geofences.Queries;

public record GetGeofencesQuery : IRequest<IReadOnlyList<GeofenceDto>>;

public class GetGeofencesQueryHandler : IRequestHandler<GetGeofencesQuery, IReadOnlyList<GeofenceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGeofencesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<GeofenceDto>> Handle(GetGeofencesQuery request, CancellationToken ct)
    {
        var geofences = await _context.Geofences
            .AsNoTracking()
            .OrderBy(g => g.Code)
            .ToListAsync(ct);

        return geofences.Select(g => new GeofenceDto
        {
            Id = g.Id,
            Code = g.Code,
            Name = g.Name,
            Type = g.Type,
            Vertices = g.Vertices.Select(v => new GeoPointDto
            {
                Latitude = v.Latitude,
                Longitude = v.Longitude
            }).ToList(),
            IsActive = g.IsActive
        }).ToList();
    }
}

public record GetGeofenceByIdQuery(Guid Id) : IRequest<GeofenceDto?>;

public class GetGeofenceByIdQueryHandler : IRequestHandler<GetGeofenceByIdQuery, GeofenceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetGeofenceByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<GeofenceDto?> Handle(GetGeofenceByIdQuery request, CancellationToken ct)
    {
        var g = await _context.Geofences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (g is null) return null;

        return new GeofenceDto
        {
            Id = g.Id,
            Code = g.Code,
            Name = g.Name,
            Type = g.Type,
            Vertices = g.Vertices.Select(v => new GeoPointDto
            {
                Latitude = v.Latitude,
                Longitude = v.Longitude
            }).ToList(),
            IsActive = g.IsActive
        };
    }
}

/// <summary>
/// Checks a delivery stop against all active geofences and returns the ones
/// the stop falls inside. Uses ray-casting point-in-polygon.
/// </summary>
public record CheckStopAgainstGeofencesQuery(Guid StopId) : IRequest<IReadOnlyList<GeofenceCheckResultDto>>;

public class CheckStopAgainstGeofencesQueryHandler
    : IRequestHandler<CheckStopAgainstGeofencesQuery, IReadOnlyList<GeofenceCheckResultDto>>
{
    private readonly IApplicationDbContext _context;

    public CheckStopAgainstGeofencesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<GeofenceCheckResultDto>> Handle(
        CheckStopAgainstGeofencesQuery request, CancellationToken ct)
    {
        var stop = await _context.DeliveryStops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StopId, ct);

        if (stop is null || !stop.Latitude.HasValue || !stop.Longitude.HasValue)
            return Array.Empty<GeofenceCheckResultDto>();

        var stopPoint = new GeoPoint(stop.Latitude.Value, stop.Longitude.Value);

        var geofences = await _context.Geofences
            .AsNoTracking()
            .Where(g => g.IsActive)
            .ToListAsync(ct);

        return geofences
            .Where(g => g.Vertices.Count >= 3 && GeoUtils.IsPointInPolygon(stopPoint, g.Vertices))
            .Select(g => new GeofenceCheckResultDto
            {
                GeofenceId = g.Id,
                Code = g.Code,
                Name = g.Name,
                Type = g.Type,
                IsInside = true
            })
            .ToList();
    }
}
