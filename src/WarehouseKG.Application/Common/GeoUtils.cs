using WarehouseKG.Domain.Common;

namespace WarehouseKG.Application.Common;

/// <summary>
/// Geospatial utility using the Haversine formula for distance calculations
/// and ray-casting for point-in-polygon checks. All distances in meters.
/// </summary>
public static class GeoUtils
{
    private const double EarthRadiusMeters = 6_371_000.0;

    /// <summary>
    /// Calculates the great-circle distance between two points on Earth using
    /// the Haversine formula. Returns distance in meters.
    /// </summary>
    public static double HaversineDistance(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Determines whether a point lies inside a polygon using the
    /// ray-casting algorithm. Works with geographic coordinates
    /// by treating them as Cartesian (sufficient for city-scale polygons).
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="vertices">Polygon vertices (at least 3).</param>
    /// <returns>True if the point is inside the polygon.</returns>
    public static bool IsPointInPolygon(GeoPoint point, IReadOnlyList<GeoPoint> vertices)
    {
        if (vertices.Count < 3) return false;

        var inside = false;
        var n = vertices.Count;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var vi = vertices[i];
            var vj = vertices[j];

            // Ray-casting: check if the point's horizontal ray intersects this edge
            if ((vi.Longitude > point.Longitude) != (vj.Longitude > point.Longitude) &&
                point.Latitude < (vj.Latitude - vi.Latitude) * (point.Longitude - vi.Longitude)
                    / (vj.Longitude - vi.Longitude) + vi.Latitude)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>
    /// Finds the bounding box center of a polygon (average of all vertices).
    /// </summary>
    public static GeoPoint PolygonCenter(IReadOnlyList<GeoPoint> vertices)
    {
        if (vertices.Count == 0) return new GeoPoint(0, 0);

        double sumLat = 0, sumLon = 0;
        foreach (var v in vertices)
        {
            sumLat += v.Latitude;
            sumLon += v.Longitude;
        }
        return new GeoPoint(sumLat / vertices.Count, sumLon / vertices.Count);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
