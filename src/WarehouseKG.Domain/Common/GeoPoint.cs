namespace WarehouseKG.Domain.Common;

/// <summary>
/// A geographic point defined by latitude and longitude.
/// </summary>
public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public GeoPoint() { }

    public GeoPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}