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

    public static readonly IReadOnlyList<string> All = new[]
    {
        Warehouses, InventoryItems, ItemCategories, UnitsOfMeasure,
        StockReceipts, PickOrders, PackOrders, StockTransfers,
        StockAdjustments, StockAudits,
        Suppliers, PurchaseOrders, Customers, SalesOrders,
        Reports, Users
    };
}
