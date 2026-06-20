using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeofenceType
{
    Depot = 0,
    DeliveryZone = 1,
    Restricted = 2,
    NoGo = 3
}
