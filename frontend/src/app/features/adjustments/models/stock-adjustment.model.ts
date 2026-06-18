/** Matches the backend `StockAdjustmentReason` enum. */
export type StockAdjustmentReason =
  | 'Correction'
  | 'Damage'
  | 'Loss'
  | 'Theft'
  | 'Found'
  | 'Expired'
  | 'Other';

/** Matches the backend `StockOperationStatus` enum. */
export type StockOperationStatus = 'Draft' | 'Completed' | 'Cancelled';

// ─── Stock Adjustment DTOs ────────────────────────────────────────────────

export interface StockAdjustmentLine {
  id: string;
  inventoryItemId: string;
  inventoryItemName?: string | null;
  quantityChange: number; // signed: positive adds, negative removes
  notes?: string | null;
}

export interface StockAdjustmentSummary {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  reason: StockAdjustmentReason;
  status: StockOperationStatus;
  adjustedAtUtc?: string | null;
  createdAt: string;
  lineCount: number;
}

export interface StockAdjustment {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName?: string | null;
  reason: StockAdjustmentReason;
  status: StockOperationStatus;
  adjustedAtUtc?: string | null;
  notes?: string | null;
  lines: StockAdjustmentLine[];
}

// ─── Create Request ───────────────────────────────────────────────────────

export interface StockAdjustmentLineInput {
  inventoryItemId: string;
  quantityChange: number;
  notes?: string | null;
}

export interface CreateStockAdjustmentRequest {
  number: string;
  warehouseId: string;
  reason: StockAdjustmentReason;
  adjustedAtUtc?: string | null;
  notes?: string | null;
  lines: StockAdjustmentLineInput[];
}
