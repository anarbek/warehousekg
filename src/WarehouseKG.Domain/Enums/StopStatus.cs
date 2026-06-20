using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StopStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3
}
