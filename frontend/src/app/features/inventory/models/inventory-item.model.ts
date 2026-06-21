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
  unitPrice: number;
  isActive: boolean;
}

export interface CreateInventoryItemRequest {
  sku: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasureId: string;
  reorderLevel: number;
  unitPrice: number;
  isActive: boolean;
  warehouseId?: string | null;
  initialQuantity?: number;
}

export interface UpdateInventoryItemRequest {
  sku: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasureId: string;
  reorderLevel: number;
  unitPrice: number;
  isActive: boolean;
}

export interface ItemCategory {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  parentId?: string | null;
  parentName?: string | null;
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
  isActive: boolean;
  parentId?: string | null;
}

export type UpdateItemCategoryRequest = CreateItemCategoryRequest;

export interface CreateUnitOfMeasureRequest {
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
}

export type UpdateUnitOfMeasureRequest = CreateUnitOfMeasureRequest;
