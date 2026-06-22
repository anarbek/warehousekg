using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
}
