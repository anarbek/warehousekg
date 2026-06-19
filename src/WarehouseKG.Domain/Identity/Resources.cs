namespace WarehouseKG.Domain.Identity;

public static class Resources
{
    public const string Warehouses = "warehouses";
    public const string InventoryItems = "inventory-items";
    public const string ItemCategories = "item-categories";
    public const string UnitsOfMeasure = "units-of-measure";
    public const string StockReceipts = "stock-receipts";
    public const string PickOrders = "pick-orders";
    public const string PackOrders = "pack-orders";
    public const string StockTransfers = "stock-transfers";
    public const string StockAdjustments = "stock-adjustments";
    public const string StockAudits = "stock-audits";
    public const string Suppliers = "suppliers";
    public const string PurchaseOrders = "purchase-orders";
    public const string Customers = "customers";
    public const string SalesOrders = "sales-orders";
    public const string Reports = "reports";
    public const string Users = "users";

    /// <summary>Special resource for back-in-time date permissions (not CRUD).</summary>
    public const string AddItemsBackInTime = "add-items-back-in-time";

    /// <summary>Permission to delete completed stock receipts.</summary>
    public const string ReceivingDelete = "stock-receipts-delete-completed";

    // Personnel resources
    public const string Employees = "employees";
    public const string Positions = "positions";
    public const string Departments = "departments";
    public const string Shifts = "shifts";
    public const string Attendance = "attendance";

    // Vehicle / Fleet resources
    public const string Vehicles = "vehicles";
    public const string VehicleTypes = "vehicle-types";
    public const string Maintenance = "maintenance";
    public const string Insurance = "insurance";
    public const string Inspections = "inspections";
    public const string FuelLogs = "fuel-logs";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Warehouses, InventoryItems, ItemCategories, UnitsOfMeasure,
        StockReceipts, PickOrders, PackOrders, StockTransfers,
        StockAdjustments, StockAudits,
        Suppliers, PurchaseOrders, Customers, SalesOrders,
        Reports, Users, AddItemsBackInTime, ReceivingDelete,
        Employees, Positions, Departments, Shifts, Attendance,
        Vehicles, VehicleTypes, Maintenance, Insurance, Inspections, FuelLogs
    };
}
