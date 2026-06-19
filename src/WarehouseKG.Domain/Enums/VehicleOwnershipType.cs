using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleOwnershipType
{
    Owned = 0,
    Leased = 1,
    Rented = 2
}
