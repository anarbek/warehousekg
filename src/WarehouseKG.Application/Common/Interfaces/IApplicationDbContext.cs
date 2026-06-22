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

    DbSet<TenantPermission> TenantPermissions { get; }

    DbSet<Position> Positions { get; }
    DbSet<Department> Departments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments { get; }
    DbSet<EmployeeWarehouseAssignment> EmployeeWarehouseAssignments { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<VehicleType> VehicleTypes { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleDriverAssignment> VehicleDriverAssignments { get; }
    DbSet<VehicleMaintenanceRecord> VehicleMaintenanceRecords { get; }
    DbSet<VehicleInsuranceRecord> VehicleInsuranceRecords { get; }
    DbSet<VehicleInspectionRecord> VehicleInspectionRecords { get; }
    DbSet<VehicleFuelRecord> VehicleFuelRecords { get; }

    DbSet<DeliveryRoute> DeliveryRoutes { get; }
    DbSet<DeliveryStop> DeliveryStops { get; }
    DbSet<DeliveryShipment> DeliveryShipments { get; }
    DbSet<Geofence> Geofences { get; }

    DbSet<Tenant> Tenants { get; }

    DbSet<PaymentType> PaymentTypes { get; }
    DbSet<PreOrder> PreOrders { get; }
    DbSet<PreOrderLine> PreOrderLines { get; }

    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
