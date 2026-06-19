/** Matches the backend `StockOperationStatus` enum. */
export type StockOperationStatus = 'Draft' | 'Completed' | 'Cancelled';

// ─── Stock Audit DTOs ─────────────────────────────────────────────────────

export interface StockAuditLine {
  id: string;
  inventoryItemId: string;
  inventoryItemName?: string | null;
  systemQuantity: number;   // on-hand at time of audit creation
  countedQuantity: number;  // physically counted
  variance: number;         // countedQuantity − systemQuantity
}

export interface StockAuditSummary {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  status: StockOperationStatus;
  reconciledAtUtc?: string | null;
  createdAt: string;
  lineCount: number;
  totalVariance: number;
}

export interface StockAudit {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  status: StockOperationStatus;
  reconciledAtUtc?: string | null;
  notes?: string | null;
  lines: StockAuditLine[];
}

// ─── Create Request ───────────────────────────────────────────────────────

export interface StockAuditLineInput {
  inventoryItemId: string;
  countedQuantity: number;
}

export interface CreateStockAuditRequest {
  number: string;
  warehouseId: string;
  reconciledAtUtc?: string | null;
  notes?: string | null;
  employeeId?: string | null;
  lines: StockAuditLineInput[];
}
