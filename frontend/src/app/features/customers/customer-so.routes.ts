import { Routes } from '@angular/router';
export const customerRoutes: Routes = [
  { path: '', redirectTo: 'customers', pathMatch: 'full' },
  { path: 'customers', loadComponent: () => import('./customer-list/customer-list').then(m => m.CustomerList) },
  { path: 'customers/new', loadComponent: () => import('./customer-form/customer-form').then(m => m.CustomerForm) },
  { path: 'customers/:id/edit', loadComponent: () => import('./customer-form/customer-form').then(m => m.CustomerForm) },
  { path: 'sales-orders', loadComponent: () => import('./sales-orders/so-list/so-list').then(m => m.SoList) },
  { path: 'sales-orders/new', loadComponent: () => import('./sales-orders/so-form/so-form').then(m => m.SoForm) },
  { path: 'sales-orders/:id', loadComponent: () => import('./sales-orders/so-detail/so-detail').then(m => m.SoDetail) },
];
