using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Shipped = 2,
    Cancelled = 3
}
