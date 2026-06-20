using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Geofences.Queries;

public class GeofenceDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GeofenceType Type { get; set; }
    public List<GeoPointDto> Vertices { get; set; } = new();
    public bool IsActive { get; set; }
}

public class GeoPointDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class GeofenceCheckResultDto
{
    public Guid GeofenceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GeofenceType Type { get; set; }
    public bool IsInside { get; set; }
}
