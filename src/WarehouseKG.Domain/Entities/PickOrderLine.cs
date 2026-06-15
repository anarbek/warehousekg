using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class PickOrderLine : BaseEntity
{
    public Guid PickOrderId { get; set; }

    public PickOrder? PickOrder { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public Guid? WarehouseLocationId { get; set; }

    public WarehouseLocation? WarehouseLocation { get; set; }

    public decimal Quantity { get; set; }
}
