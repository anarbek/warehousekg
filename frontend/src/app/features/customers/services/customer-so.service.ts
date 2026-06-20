import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { CreateCustomerRequest, CreateSalesOrderRequest, Customer, SalesOrder, SalesOrderSummary, UpdateCustomerRequest } from '../models/customer-so.model';

@Injectable({ providedIn: 'root' })
export class CustomerSoService {
  private readonly http = inject(HttpClient);
  private readonly customersUrl = `${AppSettings.apiBaseUrl}/customers`;
  private readonly soUrl = `${AppSettings.apiBaseUrl}/sales-orders`;
  getCustomers(): Observable<Customer[]> { return this.http.get<Customer[]>(this.customersUrl); }
  getCustomerById(id: string): Observable<Customer> { return this.http.get<Customer>(`${this.customersUrl}/${id}`); }
  createCustomer(r: CreateCustomerRequest): Observable<string> { return this.http.post<string>(this.customersUrl, r); }
  updateCustomer(id: string, r: UpdateCustomerRequest): Observable<void> { return this.http.put<void>(`${this.customersUrl}/${id}`, r); }
  deleteCustomer(id: string): Observable<void> { return this.http.delete<void>(`${this.customersUrl}/${id}`); }
  getSalesOrders(): Observable<SalesOrderSummary[]> { return this.http.get<SalesOrderSummary[]>(this.soUrl); }
  getSalesOrderById(id: string): Observable<SalesOrder> { return this.http.get<SalesOrder>(`${this.soUrl}/${id}`); }
  createSalesOrder(r: CreateSalesOrderRequest): Observable<string> { return this.http.post<string>(this.soUrl, r); }
  updateSalesOrder(id: string, r: CreateSalesOrderRequest): Observable<void> { return this.http.put<void>(`${this.soUrl}/${id}`, { ...r, id }); }
  confirmSO(id: string): Observable<void> { return this.http.post<void>(`${this.soUrl}/${id}/confirm`, {}); }
  shipSO(id: string): Observable<void> { return this.http.post<void>(`${this.soUrl}/${id}/ship`, {}); }
  cancelSO(id: string): Observable<void> { return this.http.post<void>(`${this.soUrl}/${id}/cancel`, {}); }
  deleteSalesOrder(id: string): Observable<void> { return this.http.delete<void>(`${this.soUrl}/${id}`); }
}
