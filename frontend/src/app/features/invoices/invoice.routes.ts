import { Routes } from '@angular/router';

export const invoiceRoutes: Routes = [
  { path: '', redirectTo: 'list', pathMatch: 'full' },
  { path: 'list', loadComponent: () => import('./invoice-list/invoice-list').then((m) => m.InvoiceList) },
  { path: 'new', loadComponent: () => import('./invoice-form/invoice-form').then((m) => m.InvoiceForm) },
  { path: ':id/edit', loadComponent: () => import('./invoice-form/invoice-form').then((m) => m.InvoiceForm) },
  { path: ':id', loadComponent: () => import('./invoice-detail/invoice-detail').then((m) => m.InvoiceDetail) },
];
