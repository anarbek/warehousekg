using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StockOperationStatus
{
    Draft = 0,
    Completed = 1,
    Cancelled = 2
}
