import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  CreatePurchaseOrderRequest,
  CreateSupplierRequest,
  PurchaseOrder,
  PurchaseOrderSummary,
  Supplier,
  UpdateSupplierRequest,
} from '../models/supplier-po.model';

@Injectable({ providedIn: 'root' })
export class SupplierPoService {
  private readonly http = inject(HttpClient);
  private readonly suppliersUrl = `${AppSettings.apiBaseUrl}/suppliers`;
  private readonly poUrl = `${AppSettings.apiBaseUrl}/purchase-orders`;

  // ─── Suppliers ────────────────────────────────────────────────────────────

  getSuppliers(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(this.suppliersUrl);
  }
  getSupplierById(id: string): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.suppliersUrl}/${id}`);
  }
  createSupplier(r: CreateSupplierRequest): Observable<string> {
    return this.http.post<string>(this.suppliersUrl, r);
  }
  updateSupplier(id: string, r: UpdateSupplierRequest): Observable<void> {
    return this.http.put<void>(`${this.suppliersUrl}/${id}`, r);
  }
  deleteSupplier(id: string): Observable<void> {
    return this.http.delete<void>(`${this.suppliersUrl}/${id}`);
  }

  // ─── Purchase Orders ──────────────────────────────────────────────────────

  getPurchaseOrders(): Observable<PurchaseOrderSummary[]> {
    return this.http.get<PurchaseOrderSummary[]>(this.poUrl);
  }
  getPurchaseOrderById(id: string): Observable<PurchaseOrder> {
    return this.http.get<PurchaseOrder>(`${this.poUrl}/${id}`);
  }
  createPurchaseOrder(r: CreatePurchaseOrderRequest): Observable<string> {
    return this.http.post<string>(this.poUrl, r);
  }
  submitPO(id: string): Observable<void> {
    return this.http.post<void>(`${this.poUrl}/${id}/submit`, {});
  }
  receivePO(id: string): Observable<void> {
    return this.http.post<void>(`${this.poUrl}/${id}/receive`, {});
  }
  cancelPO(id: string): Observable<void> {
    return this.http.post<void>(`${this.poUrl}/${id}/cancel`, {});
  }
}
