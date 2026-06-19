using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleStatus
{
    Active = 0,
    InMaintenance = 1,
    OutOfService = 2,
    Decommissioned = 3
}
