import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { CreateInvoiceRequest, Invoice, InvoiceSummary } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly http = inject(HttpClient);
  private readonly url = `${AppSettings.apiBaseUrl}/invoices`;

  getInvoices(params?: { status?: string; customerId?: string; warehouseId?: string; salesOrderId?: string; dateFrom?: string; dateTo?: string }): Observable<InvoiceSummary[]> {
    let q = '';
    if (params) {
      const parts: string[] = [];
      if (params.status) parts.push(`status=${params.status}`);
      if (params.customerId) parts.push(`customerId=${params.customerId}`);
      if (params.warehouseId) parts.push(`warehouseId=${params.warehouseId}`);
      if (params.salesOrderId) parts.push(`salesOrderId=${params.salesOrderId}`);
      if (params.dateFrom) parts.push(`dateFrom=${params.dateFrom}`);
      if (params.dateTo) parts.push(`dateTo=${params.dateTo}`);
      if (parts.length) q = '?' + parts.join('&');
    }
    return this.http.get<InvoiceSummary[]>(`${this.url}${q}`);
  }

  getInvoiceById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.url}/${id}`);
  }

  createInvoice(r: CreateInvoiceRequest): Observable<string> {
    return this.http.post<string>(this.url, r);
  }

  updateInvoice(id: string, r: CreateInvoiceRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, { ...r, id });
  }

  issueInvoice(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/issue`, {});
  }

  printInvoice(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/print`, {});
  }

  signInvoice(id: string, signedByName?: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/sign`, { id, signedByName });
  }

  cancelInvoice(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/cancel`, {});
  }

  deleteInvoice(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getInvoicePdfUrl(id: string): string {
    return `${this.url}/${id}/pdf`;
  }

  downloadInvoicePdf(id: string): Observable<Blob> {
    return this.http.get(`${this.url}/${id}/pdf`, { responseType: 'blob' });
  }

  getInvoicesBySalesOrder(soId: string): Observable<InvoiceSummary[]> {
    return this.http.get<InvoiceSummary[]>(`${this.url}?salesOrderId=${soId}`);
  }
}
