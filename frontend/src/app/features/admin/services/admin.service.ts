import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';

export interface TenantPermission {
  id: string;
  roleName: string;
  resource: string;
  canRead: boolean;
  canWrite: boolean;
  canDelete: boolean;
}

export interface ResourceInfo {
  key: string;
  label: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${AppSettings.apiBaseUrl}/tenant-permissions`;

  getPermissions(): Observable<TenantPermission[]> {
    return this.http.get<TenantPermission[]>(this.base);
  }

  upsertPermission(dto: Omit<TenantPermission, 'id'> & { id?: string }): Observable<void> {
    return this.http.put<void>(this.base, dto);
  }

  bulkUpsertPermissions(dtos: (Omit<TenantPermission, 'id'> & { id?: string })[]): Observable<void> {
    return this.http.put<void>(`${this.base}/bulk`, dtos);
  }

  deletePermission(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/roles`);
  }

  getResources(): Observable<ResourceInfo[]> {
    return this.http.get<ResourceInfo[]>(`${this.base}/resources`);
  }
}
