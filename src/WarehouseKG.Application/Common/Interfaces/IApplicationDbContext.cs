using Microsoft.EntityFrameworkCore;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Warehouse> Warehouses { get; }

    DbSet<WarehouseLocation> WarehouseLocations { get; }

    DbSet<ItemCategory> ItemCategories { get; }

    DbSet<UnitOfMeasure> UnitsOfMeasure { get; }

    DbSet<InventoryItem> InventoryItems { get; }

    DbSet<StockReceipt> StockReceipts { get; }

    DbSet<StockReceiptLine> StockReceiptLines { get; }

    DbSet<PickOrder> PickOrders { get; }

    DbSet<PickOrderLine> PickOrderLines { get; }

    DbSet<PackOrder> PackOrders { get; }

    DbSet<PackOrderLine> PackOrderLines { get; }

    DbSet<StockTransfer> StockTransfers { get; }

    DbSet<StockTransferLine> StockTransferLines { get; }

    DbSet<Supplier> Suppliers { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }

    DbSet<Customer> Customers { get; }

    DbSet<SalesOrder> SalesOrders { get; }

    DbSet<SalesOrderLine> SalesOrderLines { get; }

    DbSet<StockAdjustment> StockAdjustments { get; }

    DbSet<StockAdjustmentLine> StockAdjustmentLines { get; }

    DbSet<StockAudit> StockAudits { get; }

    DbSet<StockAuditLine> StockAuditLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
