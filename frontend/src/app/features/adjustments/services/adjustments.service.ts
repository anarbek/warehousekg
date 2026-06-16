import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { CreateStockAdjustmentRequest, StockAdjustment, StockAdjustmentSummary, StockAudit, StockAuditSummary } from '../models/adjustment.model';

@Injectable({ providedIn: 'root' })
export class AdjustmentsService {
  private readonly http = inject(HttpClient);
  private readonly adjUrl = `${AppSettings.apiBaseUrl}/stock-adjustments`;
  private readonly audUrl = `${AppSettings.apiBaseUrl}/stock-audits`;

  getAdjustments(): Observable<StockAdjustmentSummary[]> { return this.http.get<StockAdjustmentSummary[]>(this.adjUrl); }
  getAdjustmentById(id: string): Observable<StockAdjustment> { return this.http.get<StockAdjustment>(`${this.adjUrl}/${id}`); }
  createAdjustment(r: CreateStockAdjustmentRequest): Observable<string> { return this.http.post<string>(this.adjUrl, r); }
  completeAdjustment(id: string): Observable<void> { return this.http.post<void>(`${this.adjUrl}/${id}/complete`, {}); }
  cancelAdjustment(id: string): Observable<void> { return this.http.post<void>(`${this.adjUrl}/${id}/cancel`, {}); }

  getAudits(): Observable<StockAuditSummary[]> { return this.http.get<StockAuditSummary[]>(this.audUrl); }
  getAuditById(id: string): Observable<StockAudit> { return this.http.get<StockAudit>(`${this.audUrl}/${id}`); }
}
