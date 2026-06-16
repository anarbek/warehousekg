import { Routes } from '@angular/router';

export const supplierRoutes: Routes = [
  { path: '', redirectTo: 'suppliers', pathMatch: 'full' },
  { path: 'suppliers', loadComponent: () => import('./supplier-list/supplier-list').then(m => m.SupplierList) },
  { path: 'suppliers/new', loadComponent: () => import('./supplier-form/supplier-form').then(m => m.SupplierForm) },
  { path: 'suppliers/:id/edit', loadComponent: () => import('./supplier-form/supplier-form').then(m => m.SupplierForm) },
  { path: 'purchase-orders', loadComponent: () => import('./purchase-orders/po-list/po-list').then(m => m.PoList) },
  { path: 'purchase-orders/new', loadComponent: () => import('./purchase-orders/po-form/po-form').then(m => m.PoForm) },
  { path: 'purchase-orders/:id', loadComponent: () => import('./purchase-orders/po-detail/po-detail').then(m => m.PoDetail) },
];
