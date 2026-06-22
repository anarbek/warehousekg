using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Invoices.Dtos;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }

    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public DateTime IssuedAtUtc { get; set; }
    public DateTime? PrintedAtUtc { get; set; }
    public DateTime? SignedAtUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "KGS";
    public decimal ExchangeRate { get; set; } = 1m;
    public string? PaymentType { get; set; }

    public string? PrintedBy { get; set; }
    public string? SignedByName { get; set; }
    public string? SignatureDataUrl { get; set; }

    public string? Notes { get; set; }
    public string? ExternalReference { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<InvoiceLineDto> Lines { get; set; } = new();
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public string? InventoryItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "KGS";
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public int LineCount { get; set; }
}
