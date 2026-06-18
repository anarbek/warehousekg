namespace WarehouseKG.Application.Features.Reports.Dtos;

/// <summary>
/// A single movement event for an inventory item at a warehouse.
/// </summary>
public class ItemMovementDto
{
    /// <summary>Timestamp of the operation.</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Operation type name (e.g. "StockReceipt", "PickOrder").</summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>Document number.</summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>Document ID for navigation.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Quantity added (positive) or removed (negative).</summary>
    public decimal QuantityChange { get; set; }

    /// <summary>Reason or notes for the operation.</summary>
    public string? Notes { get; set; }

    /// <summary>Running balance after this operation.</summary>
    public decimal RunningBalance { get; set; }

    /// <summary>When the source record was created.</summary>
    public DateTime CreatedAt { get; set; }
}
