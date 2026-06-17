import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreatePickOrderRequest,
  CreatePackOrderRequest,
  CreateStockReceiptRequest,
  CreateStockTransferRequest,
  PackOrder,
  PickOrder,
  StockReceipt,
  StockTransfer,
} from '../models/stock-operation.model';

@Injectable({ providedIn: 'root' })
export class StockOperationsService {
  private readonly http = inject(HttpClient);

  private readonly receiptsUrl = `${AppSettings.apiBaseUrl}/stock-receipts`;
  private readonly picksUrl = `${AppSettings.apiBaseUrl}/pick-orders`;
  private readonly packsUrl = `${AppSettings.apiBaseUrl}/pack-orders`;
  private readonly transfersUrl = `${AppSettings.apiBaseUrl}/stock-transfers`;

  // ─── Stock Receipts ─────────────────────────────────────────────────────

  getStockReceipts(): Observable<StockReceipt[]> {
    return this.http.get<StockReceipt[]>(this.receiptsUrl);
  }

  getStockReceiptById(id: string): Observable<StockReceipt> {
    return this.http.get<StockReceipt>(`${this.receiptsUrl}/${id}`);
  }

  createStockReceipt(request: CreateStockReceiptRequest): Observable<string> {
    return this.http.post<string>(this.receiptsUrl, request);
  }

  completeStockReceipt(id: string): Observable<void> {
    return this.http.post<void>(`${this.receiptsUrl}/${id}/complete`, {});
  }

  cancelStockReceipt(id: string): Observable<void> {
    return this.http.post<void>(`${this.receiptsUrl}/${id}/cancel`, {});
  }

  deleteStockReceipt(id: string): Observable<void> {
    return this.http.delete<void>(`${this.receiptsUrl}/${id}`);
  }

  // ─── Pick Orders ────────────────────────────────────────────────────────

  getPickOrders(): Observable<PickOrder[]> {
    return this.http.get<PickOrder[]>(this.picksUrl);
  }

  getPickOrderById(id: string): Observable<PickOrder> {
    return this.http.get<PickOrder>(`${this.picksUrl}/${id}`);
  }

  createPickOrder(request: CreatePickOrderRequest): Observable<string> {
    return this.http.post<string>(this.picksUrl, request);
  }

  completePickOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.picksUrl}/${id}/complete`, {});
  }

  cancelPickOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.picksUrl}/${id}/cancel`, {});
  }

  // ─── Pack Orders ────────────────────────────────────────────────────────

  getPackOrders(): Observable<PackOrder[]> {
    return this.http.get<PackOrder[]>(this.packsUrl);
  }

  getPackOrderById(id: string): Observable<PackOrder> {
    return this.http.get<PackOrder>(`${this.packsUrl}/${id}`);
  }

  createPackOrder(request: CreatePackOrderRequest): Observable<string> {
    return this.http.post<string>(this.packsUrl, request);
  }

  completePackOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.packsUrl}/${id}/complete`, {});
  }

  cancelPackOrder(id: string): Observable<void> {
    return this.http.post<void>(`${this.packsUrl}/${id}/cancel`, {});
  }

  // ─── Stock Transfers ────────────────────────────────────────────────────

  getStockTransfers(): Observable<StockTransfer[]> {
    return this.http.get<StockTransfer[]>(this.transfersUrl);
  }

  getStockTransferById(id: string): Observable<StockTransfer> {
    return this.http.get<StockTransfer>(`${this.transfersUrl}/${id}`);
  }

  createStockTransfer(request: CreateStockTransferRequest): Observable<string> {
    return this.http.post<string>(this.transfersUrl, request);
  }

  completeStockTransfer(id: string): Observable<void> {
    return this.http.post<void>(`${this.transfersUrl}/${id}/complete`, {});
  }

  cancelStockTransfer(id: string): Observable<void> {
    return this.http.post<void>(`${this.transfersUrl}/${id}/cancel`, {});
  }
}
