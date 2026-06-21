import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppSettings } from '../../core/config/app-settings';
import { Tenant, CreateTenantRequest, UpdateTenantRequest } from './tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${AppSettings.apiBaseUrl}/tenants`;

  getTenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.baseUrl);
  }

  getTenantById(id: string): Observable<Tenant> {
    return this.http.get<Tenant>(`${this.baseUrl}/${id}`);
  }

  createTenant(request: CreateTenantRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateTenant(id: string, request: UpdateTenantRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  suspendTenant(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/suspend`, {});
  }

  activateTenant(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/activate`, {});
  }

  resetUserPassword(tenantId: string, userId: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${tenantId}/users/${userId}/password`, { newPassword });
  }
}
