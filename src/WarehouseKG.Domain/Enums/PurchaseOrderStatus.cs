using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PurchaseOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Received = 2,
    Cancelled = 3
}
