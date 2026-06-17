namespace WarehouseKG.Application.Features.Reports.Dtos;

/// <summary>
/// Per-item stock level at a warehouse, calculated from completed operations.
/// </summary>
public class WarehouseStockItemDto
{
    /// <summary>Inventory item ID.</summary>
    public Guid InventoryItemId { get; set; }

    /// <summary>Item SKU.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Item name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Category name.</summary>
    public string? CategoryName { get; set; }

    /// <summary>Barcode.</summary>
    public string? Barcode { get; set; }

    /// <summary>Reorder level.</summary>
    public decimal ReorderLevel { get; set; }

    /// <summary>Net quantity change from all completed operations at this warehouse within the date range.</summary>
    public decimal NetChange { get; set; }

    /// <summary>Current tenant-wide quantity on hand (for reference).</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Whether the item is active.</summary>
    public bool IsActive { get; set; }
}
