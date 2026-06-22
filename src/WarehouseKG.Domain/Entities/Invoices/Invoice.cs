using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class Invoice : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public InvoiceType Type { get; set; } = InvoiceType.Sales;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // Links
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    // Dates
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? PrintedAtUtc { get; set; }
    public DateTime? SignedAtUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }

    // Financial
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "KGS";
    public decimal ExchangeRate { get; set; } = 1m;
    public string? PaymentType { get; set; }

    // Print & Signature
    public string? ReportLayoutId { get; set; }
    public string? PrintedBy { get; set; }
    public string? SignedByName { get; set; }
    public string? SignatureDataUrl { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? ExternalReference { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
