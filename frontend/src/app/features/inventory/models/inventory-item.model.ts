export interface InventoryItem {
  id: string;
  sku: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasureId: string;
  quantityOnHand: number;
  reorderLevel: number;
  isActive: boolean;
}

export interface CreateInventoryItemRequest {
  sku: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasureId: string;
  quantityOnHand: number;
  reorderLevel: number;
  isActive: boolean;
}

export type UpdateInventoryItemRequest = CreateInventoryItemRequest;
