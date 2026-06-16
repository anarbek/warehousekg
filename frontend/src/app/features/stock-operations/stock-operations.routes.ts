import { Routes } from '@angular/router';
import { OperationType } from './models/stock-operation.model';

export const stockOperationsRoutes: Routes = [
  { path: '', redirectTo: 'receiving', pathMatch: 'full' },

  // ─── Receiving ──────────────────────────────────────────────────────────
  {
    path: 'receiving',
    loadComponent: () =>
      import('./stock-operation-list/stock-operation-list').then((m) => m.StockOperationList),
    data: { operationType: 'receiving' as OperationType },
  },
  {
    path: 'receiving/new',
    loadComponent: () =>
      import('./receiving-form/receiving-form').then((m) => m.ReceivingForm),
  },
  {
    path: 'receiving/:id',
    loadComponent: () =>
      import('./stock-operation-detail/stock-operation-detail').then((m) => m.StockOperationDetail),
    data: { operationType: 'receiving' as OperationType },
  },

  // ─── Picking ────────────────────────────────────────────────────────────
  {
    path: 'picking',
    loadComponent: () =>
      import('./stock-operation-list/stock-operation-list').then((m) => m.StockOperationList),
    data: { operationType: 'picking' as OperationType },
  },
  {
    path: 'picking/new',
    loadComponent: () =>
      import('./picking-form/picking-form').then((m) => m.PickingForm),
  },
  {
    path: 'picking/:id',
    loadComponent: () =>
      import('./stock-operation-detail/stock-operation-detail').then((m) => m.StockOperationDetail),
    data: { operationType: 'picking' as OperationType },
  },

  // ─── Packing ────────────────────────────────────────────────────────────
  {
    path: 'packing',
    loadComponent: () =>
      import('./stock-operation-list/stock-operation-list').then((m) => m.StockOperationList),
    data: { operationType: 'packing' as OperationType },
  },
  {
    path: 'packing/new',
    loadComponent: () =>
      import('./packing-form/packing-form').then((m) => m.PackingForm),
  },
  {
    path: 'packing/:id',
    loadComponent: () =>
      import('./stock-operation-detail/stock-operation-detail').then((m) => m.StockOperationDetail),
    data: { operationType: 'packing' as OperationType },
  },

  // ─── Transfers ──────────────────────────────────────────────────────────
  {
    path: 'transfer',
    loadComponent: () =>
      import('./stock-operation-list/stock-operation-list').then((m) => m.StockOperationList),
    data: { operationType: 'transfer' as OperationType },
  },
  {
    path: 'transfer/new',
    loadComponent: () =>
      import('./transfer-form/transfer-form').then((m) => m.TransferForm),
  },
  {
    path: 'transfer/:id',
    loadComponent: () =>
      import('./stock-operation-detail/stock-operation-detail').then((m) => m.StockOperationDetail),
    data: { operationType: 'transfer' as OperationType },
  },
];
