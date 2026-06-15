namespace WarehouseKG.Application.Features.InventoryItems.Dtos;

public class InventoryItemDto
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Barcode { get; set; }

    public Guid CategoryId { get; set; }

    public Guid UnitOfMeasureId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReorderLevel { get; set; }

    public bool IsActive { get; set; }
}
