import { Routes } from '@angular/router';

export const inventoryRoutes: Routes = [
  { path: '', redirectTo: 'warehouses', pathMatch: 'full' },

  // ─── Warehouses ─────────────────────────────────────────────────────────────
  {
    path: 'warehouses',
    loadComponent: () =>
      import('./warehouses/warehouse-list/warehouse-list').then((m) => m.WarehouseList),
  },
  {
    path: 'warehouses/new',
    loadComponent: () =>
      import('./warehouses/warehouse-form/warehouse-form').then((m) => m.WarehouseForm),
  },
  {
    path: 'warehouses/:id/edit',
    loadComponent: () =>
      import('./warehouses/warehouse-form/warehouse-form').then((m) => m.WarehouseForm),
  },
  {
    path: 'warehouses/:id',
    loadComponent: () =>
      import('./warehouses/warehouse-detail/warehouse-detail').then((m) => m.WarehouseDetail),
  },

  // ─── Inventory Items ─────────────────────────────────────────────────────────
  {
    path: 'items',
    loadComponent: () =>
      import('./inventory-items/inventory-item-list/inventory-item-list').then(
        (m) => m.InventoryItemList,
      ),
  },
  {
    path: 'items/new',
    loadComponent: () =>
      import('./inventory-items/inventory-item-form/inventory-item-form').then(
        (m) => m.InventoryItemForm,
      ),
  },
  {
    path: 'items/:id/edit',
    loadComponent: () =>
      import('./inventory-items/inventory-item-form/inventory-item-form').then(
        (m) => m.InventoryItemForm,
      ),
  },
  {
    path: 'items/:id',
    loadComponent: () =>
      import('./inventory-items/inventory-item-detail/inventory-item-detail').then(
        (m) => m.InventoryItemDetail,
      ),
  },
];
