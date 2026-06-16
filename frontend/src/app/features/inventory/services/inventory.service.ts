import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreateInventoryItemRequest,
  CreateItemCategoryRequest,
  CreateUnitOfMeasureRequest,
  InventoryItem,
  ItemCategory,
  UnitOfMeasure,
  UpdateInventoryItemRequest,
  UpdateItemCategoryRequest,
  UpdateUnitOfMeasureRequest,
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
  private readonly categoriesUrl = `${AppSettings.apiBaseUrl}/item-categories`;
  private readonly uomsUrl = `${AppSettings.apiBaseUrl}/units-of-measure`;

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

  // ─── Item Categories ───────────────────────────────────────────────────────

  getItemCategories(): Observable<ItemCategory[]> {
    return this.http.get<ItemCategory[]>(this.categoriesUrl);
  }

  getItemCategoryById(id: string): Observable<ItemCategory> {
    return this.http.get<ItemCategory>(`${this.categoriesUrl}/${id}`);
  }

  createItemCategory(request: CreateItemCategoryRequest): Observable<string> {
    return this.http.post<string>(this.categoriesUrl, request);
  }

  updateItemCategory(id: string, request: UpdateItemCategoryRequest): Observable<void> {
    return this.http.put<void>(`${this.categoriesUrl}/${id}`, request);
  }

  deleteItemCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
  }

  // ─── Units of Measure ──────────────────────────────────────────────────────

  getUnitsOfMeasure(): Observable<UnitOfMeasure[]> {
    return this.http.get<UnitOfMeasure[]>(this.uomsUrl);
  }

  getUnitOfMeasureById(id: string): Observable<UnitOfMeasure> {
    return this.http.get<UnitOfMeasure>(`${this.uomsUrl}/${id}`);
  }

  createUnitOfMeasure(request: CreateUnitOfMeasureRequest): Observable<string> {
    return this.http.post<string>(this.uomsUrl, request);
  }

  updateUnitOfMeasure(id: string, request: UpdateUnitOfMeasureRequest): Observable<void> {
    return this.http.put<void>(`${this.uomsUrl}/${id}`, request);
  }

  deleteUnitOfMeasure(id: string): Observable<void> {
    return this.http.delete<void>(`${this.uomsUrl}/${id}`);
  }
}
