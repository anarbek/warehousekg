import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'auth/callback',
    loadComponent: () =>
      import('./features/auth/callback/auth-callback').then((m) => m.AuthCallback),
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell').then((m) => m.Shell),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'inventory',
        loadChildren: () =>
          import('./features/inventory/inventory.routes').then((m) => m.inventoryRoutes),
      },
      {
        path: 'stock-operations',
        loadChildren: () =>
          import('./features/stock-operations/stock-operations.routes').then(
            (m) => m.stockOperationsRoutes,
          ),
      },
      {
        path: 'suppliers',
        loadChildren: () =>
          import('./features/suppliers/supplier-po.routes').then((m) => m.supplierRoutes),
      },
      {
        path: 'customers',
        loadChildren: () =>
          import('./features/customers/customer-so.routes').then((m) => m.customerRoutes),
      },
      {
        path: 'adjustments',
        loadChildren: () =>
          import('./features/adjustments/adjustments.routes').then((m) => m.adjustmentRoutes),
      },
      {
        path: 'reports',
        loadChildren: () =>
          import('./features/reports/reports.routes').then((m) => m.reportRoutes),
      },
      {
        path: 'admin',
        loadChildren: () =>
          import('./features/admin/admin.routes').then((m) => m.adminRoutes),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
