using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MaintenanceType
{
    OilChange = 0,
    TireChange = 1,
    BrakeService = 2,
    EngineService = 3,
    TransmissionService = 4,
    GeneralCheck = 5,
    Repair = 6,
    Other = 7
}
