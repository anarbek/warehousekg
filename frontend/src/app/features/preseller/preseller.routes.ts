import { Routes } from '@angular/router';

export const presellerRoutes: Routes = [
  {
    path: '',
    redirectTo: 'pre-orders',
    pathMatch: 'full',
  },
  {
    path: 'pre-orders',
    loadComponent: () =>
      import('./pre-order-list/pre-order-list').then((m) => m.PreOrderList),
  },
  {
    path: 'pre-orders/new',
    loadComponent: () =>
      import('./pre-order-form/pre-order-form').then((m) => m.PreOrderForm),
  },
  {
    path: 'pre-orders/:id/edit',
    loadComponent: () =>
      import('./pre-order-form/pre-order-form').then((m) => m.PreOrderForm),
  },
  {
    path: 'pre-orders/:id',
    loadComponent: () =>
      import('./pre-order-detail/pre-order-detail').then((m) => m.PreOrderDetail),
  },
];
