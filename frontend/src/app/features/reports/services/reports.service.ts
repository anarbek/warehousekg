import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { InventorySummary, LowStockItem, PurchaseSummary, SalesSummary, StockMovementSummary } from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${AppSettings.apiBaseUrl}/reports`;

  getInventorySummary(): Observable<InventorySummary> { return this.http.get<InventorySummary>(`${this.base}/inventory-summary`); }
  getLowStock(): Observable<LowStockItem[]> { return this.http.get<LowStockItem[]>(`${this.base}/low-stock`); }
  getSalesSummary(): Observable<SalesSummary> { return this.http.get<SalesSummary>(`${this.base}/sales-summary`); }
  getPurchaseSummary(): Observable<PurchaseSummary> { return this.http.get<PurchaseSummary>(`${this.base}/purchase-summary`); }
  getStockMovements(): Observable<StockMovementSummary> { return this.http.get<StockMovementSummary>(`${this.base}/stock-movements`); }
}
