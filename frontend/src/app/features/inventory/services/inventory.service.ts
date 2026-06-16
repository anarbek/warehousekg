import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreateInventoryItemRequest,
  InventoryItem,
  UpdateInventoryItemRequest,
} from '../models/inventory-item.model';
import {
  CreateWarehouseRequest,
  UpdateWarehouseRequest,
  Warehouse,
} from '../models/warehouse.model';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly warehousesUrl = `${AppSettings.apiBaseUrl}/warehouses`;
  private readonly itemsUrl = `${AppSettings.apiBaseUrl}/inventory-items`;

  // ─── Warehouses ────────────────────────────────────────────────────────────

  getWarehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.warehousesUrl);
  }

  getWarehouseById(id: string): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.warehousesUrl}/${id}`);
  }

  createWarehouse(request: CreateWarehouseRequest): Observable<string> {
    return this.http.post<string>(this.warehousesUrl, request);
  }

  updateWarehouse(id: string, request: UpdateWarehouseRequest): Observable<void> {
    return this.http.put<void>(`${this.warehousesUrl}/${id}`, request);
  }

  deleteWarehouse(id: string): Observable<void> {
    return this.http.delete<void>(`${this.warehousesUrl}/${id}`);
  }

  // ─── Inventory Items ────────────────────────────────────────────────────────

  getInventoryItems(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(this.itemsUrl);
  }

  getInventoryItemById(id: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.itemsUrl}/${id}`);
  }

  createInventoryItem(request: CreateInventoryItemRequest): Observable<string> {
    return this.http.post<string>(this.itemsUrl, request);
  }

  updateInventoryItem(id: string, request: UpdateInventoryItemRequest): Observable<void> {
    return this.http.put<void>(`${this.itemsUrl}/${id}`, request);
  }

  deleteInventoryItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.itemsUrl}/${id}`);
  }
}
