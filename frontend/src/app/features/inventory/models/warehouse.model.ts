export interface Warehouse {
  id: string;
  code: string;
  name: string;
  address?: string | null;
  isActive: boolean;
}

export interface CreateWarehouseRequest {
  code: string;
  name: string;
  address?: string | null;
  isActive: boolean;
}

export type UpdateWarehouseRequest = CreateWarehouseRequest;
