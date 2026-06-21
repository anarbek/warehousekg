export enum PreOrderStatus {
  Draft = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
  Converted = 4,
}

export interface PreOrderSummary {
  id: string;
  number: string;
  customerId: string;
  customerName?: string | null;
  presellerName?: string | null;
  status: PreOrderStatus;
  paymentType: string;
  totalAmount: number;
  lineCount: number;
  orderDateUtc: string;
  createdAt: string;
}

export interface PreOrder {
  id: string;
  number: string;
  customerId: string;
  customerName?: string | null;
  presellerId: string;
  presellerName?: string | null;
  warehouseId: string;
  warehouseName?: string | null;
  status: PreOrderStatus;
  paymentType: string;
  currency: string;
  orderDateUtc: string;
  expectedDateUtc?: string | null;
  submittedAtUtc?: string | null;
  approvedAtUtc?: string | null;
  rejectedAtUtc?: string | null;
  convertedAtUtc?: string | null;
  convertedSalesOrderId?: string | null;
  convertedSalesOrderNumber?: string | null;
  notes?: string | null;
  totalAmount: number;
  lines: PreOrderLine[];
}

export interface PreOrderLine {
  id: string;
  inventoryItemId: string;
  inventoryItemName?: string | null;
  sku?: string | null;
  quantity: number;
  unitPrice: number;
  warehouseStockSnapshot: number;
  stockDifference: number;
  discountPercent: number;
  lineTotal: number;
}

export interface PreOrderLineInput {
  inventoryItemId: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
}

export interface CreatePreOrderRequest {
  number: string;
  customerId: string;
  warehouseId: string;
  paymentType: string;
  currency?: string | null;
  expectedDateUtc?: string | null;
  notes?: string | null;
  lines: PreOrderLineInput[];
}

export interface UpdatePreOrderRequest extends CreatePreOrderRequest {
  id: string;
}

export interface PaymentType {
  id: string;
  code: string;
  name: string;
}

export interface WarehouseStockItem {
  inventoryItemId: string;
  sku: string;
  name: string;
  quantityOnHand: number;
}

export interface RejectRequest {
  reason?: string | null;
}
