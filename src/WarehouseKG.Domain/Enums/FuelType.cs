using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FuelType
{
    Diesel = 0,
    Gasoline = 1,
    Electric = 2,
    Hybrid = 3,
    LPG = 4,
    CNG = 5
}
