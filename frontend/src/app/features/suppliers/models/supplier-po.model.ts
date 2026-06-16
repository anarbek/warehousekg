export interface Supplier {
  id: string;
  code: string;
  name: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  taxId?: string | null;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  code: string;
  name: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  taxId?: string | null;
  isActive: boolean;
}

export type UpdateSupplierRequest = CreateSupplierRequest;

export type PurchaseOrderStatus = 'Draft' | 'Submitted' | 'Received' | 'Cancelled';

export interface PurchaseOrderLine {
  id?: string;
  inventoryItemId: string;
  inventoryItemName?: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PurchaseOrder {
  id: string;
  number: string;
  supplierId: string;
  supplierName?: string | null;
  warehouseId?: string | null;
  warehouseName?: string | null;
  status: PurchaseOrderStatus;
  currency: string;
  orderDateUtc: string;
  expectedDateUtc?: string | null;
  submittedAtUtc?: string | null;
  receivedAtUtc?: string | null;
  notes?: string | null;
  totalAmount: number;
  lines: PurchaseOrderLine[];
}

export interface PurchaseOrderSummary {
  id: string;
  number: string;
  supplierId: string;
  supplierName?: string | null;
  warehouseName?: string | null;
  status: PurchaseOrderStatus;
  currency: string;
  orderDateUtc: string;
  totalAmount: number;
  lineCount: number;
}

export interface CreatePurchaseOrderRequest {
  number: string;
  supplierId: string;
  warehouseId?: string | null;
  currency?: string | null;
  expectedDateUtc?: string | null;
  notes?: string | null;
  lines: { inventoryItemId: string; quantity: number; unitPrice: number }[];
}
