import { Routes } from '@angular/router';
export const customerRoutes: Routes = [
  { path: '', redirectTo: 'customers', pathMatch: 'full' },
  { path: 'customers', loadComponent: () => import('./customer-list/customer-list').then(m => m.CustomerList) },
  { path: 'customers/new', loadComponent: () => import('./customer-form/customer-form').then(m => m.CustomerForm) },
  { path: 'customers/:id/edit', loadComponent: () => import('./customer-form/customer-form').then(m => m.CustomerForm) },
  { path: 'customers/:id', loadComponent: () => import('./customer-detail/customer-detail').then(m => m.CustomerDetail) },
  { path: 'sales-orders', loadComponent: () => import('./sales-orders/so-list/so-list').then(m => m.SoList) },
  { path: 'sales-orders/new', loadComponent: () => import('./sales-orders/so-form/so-form').then(m => m.SoForm) },
  { path: 'sales-orders/:id/edit', loadComponent: () => import('./sales-orders/so-form/so-form').then(m => m.SoForm) },
  { path: 'sales-orders/:id', loadComponent: () => import('./sales-orders/so-detail/so-detail').then(m => m.SoDetail) },
];
