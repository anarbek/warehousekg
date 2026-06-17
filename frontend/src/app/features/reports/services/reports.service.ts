import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { InventorySummary, ItemMovement, LowStockItem, PurchaseSummary, SalesSummary, StockMovementSummary, WarehouseStockItem } from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${AppSettings.apiBaseUrl}/reports`;

  getInventorySummary(): Observable<InventorySummary> { return this.http.get<InventorySummary>(`${this.base}/inventory-summary`); }
  getLowStock(): Observable<LowStockItem[]> { return this.http.get<LowStockItem[]>(`${this.base}/low-stock`); }
  getSalesSummary(): Observable<SalesSummary> { return this.http.get<SalesSummary>(`${this.base}/sales-summary`); }
  getPurchaseSummary(): Observable<PurchaseSummary> { return this.http.get<PurchaseSummary>(`${this.base}/purchase-summary`); }
  getStockMovements(): Observable<StockMovementSummary> { return this.http.get<StockMovementSummary>(`${this.base}/stock-movements`); }

  getWarehouseStock(warehouseId: string, dateFrom?: string, dateTo?: string): Observable<WarehouseStockItem[]> {
    let url = `${this.base}/warehouse-stock?warehouseId=${warehouseId}`;
    if (dateFrom) url += `&dateFrom=${dateFrom}`;
    if (dateTo) url += `&dateTo=${dateTo}`;
    return this.http.get<WarehouseStockItem[]>(url);
  }

  getItemMovements(itemId: string, warehouseId: string): Observable<ItemMovement[]> {
    return this.http.get<ItemMovement[]>(`${this.base}/item-movements?itemId=${itemId}&warehouseId=${warehouseId}`);
  }
}
