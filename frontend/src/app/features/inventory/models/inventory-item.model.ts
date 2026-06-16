export interface InventoryItem {
  id: string;
  sku: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  categoryName?: string | null;
  unitOfMeasureId: string;
  unitOfMeasureName?: string | null;
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

export interface ItemCategory {
  id: string;
  code: string;
  name: string;
  description?: string | null;
}

export interface UnitOfMeasure {
  id: string;
  code: string;
  name: string;
  description?: string | null;
}

export interface CreateItemCategoryRequest {
  code: string;
  name: string;
  description?: string | null;
}

export type UpdateItemCategoryRequest = CreateItemCategoryRequest;

export interface CreateUnitOfMeasureRequest {
  code: string;
  name: string;
  description?: string | null;
}

export type UpdateUnitOfMeasureRequest = CreateUnitOfMeasureRequest;
