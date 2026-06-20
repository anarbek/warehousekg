using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class Geofence : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public GeofenceType Type { get; set; } = GeofenceType.DeliveryZone;

    /// <summary>
    /// Polygon vertices defining the geofence boundary (at least 3 points).
    /// Stored as JSONB in PostgreSQL.
    /// </summary>
    public List<GeoPoint> Vertices { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
