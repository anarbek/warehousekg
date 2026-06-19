using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InspectionResult
{
    Passed = 0,
    Failed = 1,
    RequiresRecheck = 2
}
