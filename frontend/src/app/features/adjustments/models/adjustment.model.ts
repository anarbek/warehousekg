export type StockOperationStatus = 'Draft' | 'Completed' | 'Cancelled';
export type AdjustmentReason = 'Correction' | 'Damage' | 'Loss' | 'Theft' | 'Found' | 'Expired' | 'Other';

export interface StockAdjustmentSummary {
  id: string; number: string; warehouseId: string; reason: AdjustmentReason; status: StockOperationStatus;
  adjustedAtUtc?: string | null; lineCount: number;
}

export interface StockAdjustmentLine {
  id?: string; inventoryItemId: string; inventoryItemName?: string | null; quantityChange: number; notes?: string | null;
}

export interface StockAdjustment {
  id: string; number: string; warehouseId: string; warehouseName?: string | null;
  reason: AdjustmentReason; status: StockOperationStatus; adjustedAtUtc?: string | null; notes?: string | null;
  lines: StockAdjustmentLine[];
}

export interface CreateStockAdjustmentRequest {
  number: string; warehouseId: string; reason: number; notes?: string | null;
  lines: { inventoryItemId: string; quantityChange: number; notes?: string | null }[];
}

export interface StockAuditSummary {
  id: string; number: string; warehouseId: string; status: StockOperationStatus;
  reconciledAtUtc?: string | null; lineCount: number; totalVariance: number;
}

export interface StockAuditLine {
  id?: string; inventoryItemId: string; inventoryItemName?: string | null;
  systemQuantity: number; countedQuantity: number; variance: number;
}

export interface StockAudit {
  id: string; number: string; warehouseId: string; warehouseName?: string | null;
  status: StockOperationStatus; reconciledAtUtc?: string | null; notes?: string | null;
  lines: StockAuditLine[]; totalVariance: number;
}
