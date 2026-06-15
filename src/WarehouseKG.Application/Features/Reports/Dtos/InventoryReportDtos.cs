namespace WarehouseKG.Application.Features.Reports.Dtos;

public class InventorySummaryReportDto
{
    public int TotalItems { get; set; }

    public int ActiveItems { get; set; }

    public decimal TotalQuantityOnHand { get; set; }

    public int ItemsBelowReorder { get; set; }

    public int ItemsOutOfStock { get; set; }
}

public class LowStockItemDto
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReorderLevel { get; set; }

    // How far below the reorder level the item is (ReorderLevel - QuantityOnHand).
    public decimal Deficit { get; set; }
}
