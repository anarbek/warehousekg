import { Routes } from '@angular/router';
export const adjustmentRoutes: Routes = [
  { path: '', redirectTo: 'adjustments', pathMatch: 'full' },
  { path: 'adjustments', loadComponent: () => import('./adjustment-list/adjustment-list').then(m => m.AdjustmentList) },
  { path: 'adjustments/new', loadComponent: () => import('./adjustment-form/adjustment-form').then(m => m.AdjustmentForm) },
  { path: 'adjustments/:id', loadComponent: () => import('./adjustment-detail/adjustment-detail').then(m => m.AdjustmentDetail) },
  { path: 'audits', loadComponent: () => import('./audit-list/audit-list').then(m => m.AuditList) },
  { path: 'audits/:id', loadComponent: () => import('./audit-detail/audit-detail').then(m => m.AuditDetail) },
];
