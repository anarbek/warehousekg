import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreatePreOrderRequest,
  PaymentType,
  PreOrder,
  PreOrderSummary,
  RejectRequest,
  UpdatePreOrderRequest,
  WarehouseStockItem,
} from '../models/pre-order.model';

@Injectable({ providedIn: 'root' })
export class PreOrderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${AppSettings.apiBaseUrl}/pre-orders`;
  private readonly ptUrl = `${AppSettings.apiBaseUrl}/payment-types`;

  getPreOrders(): Observable<PreOrderSummary[]> {
    return this.http.get<PreOrderSummary[]>(this.url);
  }

  getMyPreOrders(): Observable<PreOrderSummary[]> {
    return this.http.get<PreOrderSummary[]>(`${this.url}/my`);
  }

  getPreOrderById(id: string): Observable<PreOrder> {
    return this.http.get<PreOrder>(`${this.url}/${id}`);
  }

  createPreOrder(r: CreatePreOrderRequest): Observable<string> {
    return this.http.post<string>(this.url, r);
  }

  updatePreOrder(id: string, r: UpdatePreOrderRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, r);
  }

  deletePreOrder(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  submitPreOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/submit`, {});
  }

  approvePreOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/approve`, {});
  }

  rejectPreOrder(id: string, reason?: string): Observable<void> {
    const body: RejectRequest = { reason: reason || null };
    return this.http.post<void>(`${this.url}/${id}/reject`, body);
  }

  convertPreOrder(id: string): Observable<string> {
    return this.http.post<string>(`${this.url}/${id}/convert`, {});
  }

  getPaymentTypes(): Observable<PaymentType[]> {
    return this.http.get<PaymentType[]>(this.ptUrl);
  }

  getWarehouseStock(warehouseId: string): Observable<WarehouseStockItem[]> {
    return this.http.get<WarehouseStockItem[]>(`${this.url}/warehouse-stock`, {
      params: { warehouseId },
    });
  }
}
