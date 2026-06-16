export interface InventorySummary { totalItems: number; activeItems: number; totalQuantityOnHand: number; itemsBelowReorder: number; itemsOutOfStock: number; }
export interface LowStockItem { id: string; sku: string; name: string; categoryId: string; quantityOnHand: number; reorderLevel: number; deficit: number; }
export interface OrderStatusBreakdown { status: string; orderCount: number; totalAmount: number; }
export interface SalesSummary { totalOrders: number; totalAmount: number; byStatus: OrderStatusBreakdown[]; }
export interface PurchaseSummary { totalOrders: number; totalAmount: number; byStatus: OrderStatusBreakdown[]; }
export interface OperationStatusCounts { operation: string; draft: number; completed: number; cancelled: number; total: number; }
export interface StockMovementSummary { operations: OperationStatusCounts[]; }
