import { Routes } from '@angular/router';
export const adjustmentRoutes: Routes = [
  { path: '', redirectTo: 'adjustments', pathMatch: 'full' },
  {
    path: 'adjustments',
    loadComponent: () =>
      import('./stock-adjustment-list/stock-adjustment-list').then((m) => m.StockAdjustmentList),
  },
  {
    path: 'adjustments/new',
    loadComponent: () =>
      import('./stock-adjustment-form/stock-adjustment-form').then((m) => m.StockAdjustmentForm),
  },
  {
    path: 'adjustments/:id',
    loadComponent: () =>
      import('./stock-adjustment-detail/stock-adjustment-detail').then((m) => m.StockAdjustmentDetail),
  },
  {
    path: 'audits',
    loadComponent: () =>
      import('./audit-list/audit-list').then((m) => m.AuditList),
  },
  {
    path: 'audits/new',
    loadComponent: () =>
      import('./audit-form/audit-form').then((m) => m.AuditForm),
  },
  {
    path: 'audits/:id',
    loadComponent: () =>
      import('./audit-detail/audit-detail').then((m) => m.AuditDetail),
  },
];
