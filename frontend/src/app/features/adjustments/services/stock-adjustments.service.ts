import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreateStockAdjustmentRequest,
  StockAdjustment,
  StockAdjustmentSummary,
} from '../models/stock-adjustment.model';

@Injectable({ providedIn: 'root' })
export class StockAdjustmentsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${AppSettings.apiBaseUrl}/stock-adjustments`;

  getStockAdjustments(): Observable<StockAdjustmentSummary[]> {
    return this.http.get<StockAdjustmentSummary[]>(this.baseUrl);
  }

  getStockAdjustmentById(id: string): Observable<StockAdjustment> {
    return this.http.get<StockAdjustment>(`${this.baseUrl}/${id}`);
  }

  createStockAdjustment(request: CreateStockAdjustmentRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  completeStockAdjustment(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/complete`, {});
  }

  cancelStockAdjustment(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
