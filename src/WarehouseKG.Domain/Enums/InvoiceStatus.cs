using System.Text.Json.Serialization;

namespace WarehouseKG.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Printed = 2,
    Signed = 3,
    Cancelled = 4
}
