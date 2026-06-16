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

  // ─── Item Categories ─────────────────────────────────────────────────────
  {
    path: 'categories',
    loadComponent: () =>
      import('./item-categories/item-category-list/item-category-list').then(
        (m) => m.ItemCategoryList,
      ),
  },
  {
    path: 'categories/new',
    loadComponent: () =>
      import('./item-categories/item-category-form/item-category-form').then(
        (m) => m.ItemCategoryForm,
      ),
  },
  {
    path: 'categories/:id/edit',
    loadComponent: () =>
      import('./item-categories/item-category-form/item-category-form').then(
        (m) => m.ItemCategoryForm,
      ),
  },

  // ─── Units of Measure ────────────────────────────────────────────────────
  {
    path: 'units-of-measure',
    loadComponent: () =>
      import('./units-of-measure/uom-list/uom-list').then((m) => m.UomList),
  },
  {
    path: 'units-of-measure/new',
    loadComponent: () =>
      import('./units-of-measure/uom-form/uom-form').then((m) => m.UomForm),
  },
  {
    path: 'units-of-measure/:id/edit',
    loadComponent: () =>
      import('./units-of-measure/uom-form/uom-form').then((m) => m.UomForm),
  },
];
