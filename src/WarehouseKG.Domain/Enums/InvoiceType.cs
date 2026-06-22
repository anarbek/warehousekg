using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceType
{
    Sales = 0,
    Purchase = 1,
    CreditNote = 2
}
