import { Routes } from '@angular/router';

export const superadminRoutes: Routes = [
  { path: '', redirectTo: 'tenants', pathMatch: 'full' },
  {
    path: 'tenants',
    loadComponent: () => import('./tenant-list/tenant-list').then(m => m.TenantList),
  },
  {
    path: 'tenants/new',
    loadComponent: () => import('./tenant-form/tenant-form').then(m => m.TenantForm),
  },
  {
    path: 'tenants/:id',
    loadComponent: () => import('./tenant-detail/tenant-detail').then(m => m.TenantDetail),
  },
  {
    path: 'tenants/:id/edit',
    loadComponent: () => import('./tenant-form/tenant-form').then(m => m.TenantForm),
  },
];
