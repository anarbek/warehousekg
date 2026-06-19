import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreateStockAuditRequest,
  StockAudit,
  StockAuditSummary,
} from '../models/audit.model';

@Injectable({ providedIn: 'root' })
export class AuditsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${AppSettings.apiBaseUrl}/stock-audits`;

  getStockAudits(): Observable<StockAuditSummary[]> {
    return this.http.get<StockAuditSummary[]>(this.baseUrl);
  }

  getStockAuditById(id: string): Observable<StockAudit> {
    return this.http.get<StockAudit>(`${this.baseUrl}/${id}`);
  }

  createStockAudit(request: CreateStockAuditRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  completeStockAudit(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/complete`, {});
  }

  cancelStockAudit(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, {});
  }

  deleteStockAudit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
