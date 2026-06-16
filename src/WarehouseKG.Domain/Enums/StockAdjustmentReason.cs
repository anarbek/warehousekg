using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StockAdjustmentReason
{
    Correction = 0,
    Damage = 1,
    Loss = 2,
    Theft = 3,
    Found = 4,
    Expired = 5,
    Other = 6
}
