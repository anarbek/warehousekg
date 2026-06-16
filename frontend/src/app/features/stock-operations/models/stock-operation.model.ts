/** Matches the backend `StockOperationStatus` enum. */
export type StockOperationStatus = 'Draft' | 'Completed' | 'Cancelled';

/** A single line item on a stock operation document. */
export interface StockOperationLine {
  inventoryItemId: string;
  inventoryItemName?: string | null;
  quantity: number;
  warehouseLocationId?: string | null;
  warehouseLocationName?: string | null;
  packageLabel?: string | null;
}

// ─── Receiving ────────────────────────────────────────────────────────────

export interface StockReceipt {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  supplierReference?: string | null;
  notes?: string | null;
  status: StockOperationStatus;
  lineCount: number;
  receivedAtUtc?: string | null;
  createdAtUtc?: string | null;
  lines: StockOperationLine[];
}

export interface CreateStockReceiptRequest {
  number: string;
  warehouseId: string;
  supplierReference?: string | null;
  notes?: string | null;
  lines: StockOperationLine[];
}

// ─── Picking ──────────────────────────────────────────────────────────────

export interface PickOrder {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  reference?: string | null;
  notes?: string | null;
  status: StockOperationStatus;
  lineCount: number;
  createdAtUtc?: string | null;
  lines: StockOperationLine[];
}

export interface CreatePickOrderRequest {
  number: string;
  warehouseId: string;
  reference?: string | null;
  notes?: string | null;
  lines: StockOperationLine[];
}

// ─── Packing ──────────────────────────────────────────────────────────────

export interface PackOrder {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  pickOrderId?: string | null;
  notes?: string | null;
  status: StockOperationStatus;
  lineCount: number;
  createdAtUtc?: string | null;
  lines: StockOperationLine[];
}

export interface CreatePackOrderRequest {
  number: string;
  warehouseId: string;
  pickOrderId?: string | null;
  notes?: string | null;
  lines: StockOperationLine[];
}

// ─── Transfer ─────────────────────────────────────────────────────────────

export interface StockTransfer {
  id: string;
  number: string;
  sourceWarehouseId: string;
  sourceWarehouseName?: string | null;
  destinationWarehouseId: string;
  destinationWarehouseName?: string | null;
  notes?: string | null;
  status: StockOperationStatus;
  lineCount: number;
  createdAtUtc?: string | null;
  lines: StockOperationLine[];
}

export interface CreateStockTransferRequest {
  number: string;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  notes?: string | null;
  lines: StockOperationLine[];
}

// ─── Update line payload (for detail view inline edits) ──────────────────

export interface UpdateLineRequest {
  inventoryItemId: string;
  quantity: number;
  warehouseLocationId?: string | null;
  packageLabel?: string | null;
}

// ─── Operation type identifier ─────────────────────────────────────────────

export type OperationType = 'receiving' | 'picking' | 'packing' | 'transfer';
