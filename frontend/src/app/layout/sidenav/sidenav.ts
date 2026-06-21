import { Component, inject, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DxTreeViewModule } from 'devextreme-angular';
import { Router } from '@angular/router';
import { AppSettings } from '../../core/config/app-settings';
import { PermissionsService, ResourcePermissions } from '../../core/services/permissions.service';

interface NavItem {
  id: string;
  text: string;
  icon?: string;
  path?: string;
  resource?: string;
  module?: string;
  requiresSuperadmin?: boolean;
  items?: NavItem[];
  expanded?: boolean;
}

@Component({
  selector: 'app-sidenav',
  imports: [DxTreeViewModule],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss',
})
export class Sidenav implements OnInit {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly perms = inject(PermissionsService);

  private readonly allItems: NavItem[] = [
    { id: 'dashboard', icon: 'chart', text: $localize`:@@nav.dashboard:Панель управления`, path: '/dashboard' },
    {
      id: 'warehouse-group', icon: 'home', module: 'inventory',
      text: $localize`:@@nav.groups.warehouse:Склад`,
      items: [
        { id: 'warehouses', icon: 'home', text: $localize`:@@nav.warehouses:Склады`, path: '/inventory/warehouses', resource: 'warehouses' },
        { id: 'items', icon: 'product', text: $localize`:@@nav.inventory:Товары`, path: '/inventory/items', resource: 'inventory-items' },
        { id: 'categories', icon: 'hierarchy', text: $localize`:@@nav.categories:Категории`, path: '/inventory/categories', resource: 'item-categories' },
        { id: 'uoms', icon: 'preferences', text: $localize`:@@nav.uoms:Ед. измерения`, path: '/inventory/units-of-measure', resource: 'units-of-measure' },
        { id: 'receiving', icon: 'import', text: $localize`:@@nav.receiving:Поступления`, path: '/stock-operations/receiving', resource: 'stock-receipts' },
        { id: 'picking', icon: 'export', text: $localize`:@@nav.picking:Сборка`, path: '/stock-operations/picking', resource: 'pick-orders' },
        { id: 'packing', icon: 'product', text: $localize`:@@nav.packing:Упаковка`, path: '/stock-operations/packing', resource: 'pack-orders' },
        { id: 'transfer', icon: 'export', text: $localize`:@@nav.transfer:Перемещения`, path: '/stock-operations/transfer', resource: 'stock-transfers' },
        { id: 'adjustments', icon: 'edit', text: $localize`:@@nav.adjustments:Корректировки`, path: '/adjustments/adjustments', resource: 'stock-adjustments' },
        { id: 'audits', icon: 'fields', text: $localize`:@@nav.audits:Аудиты`, path: '/adjustments/audits', resource: 'stock-audits' },
      ],
    },
    {
      id: 'crm-group', icon: 'user', module: 'crm',
      text: $localize`:@@nav.groups.crm:Клиенты и поставщики`,
      items: [
        { id: 'suppliers', icon: 'globe', text: $localize`:@@nav.suppliers:Поставщики`, path: '/suppliers/suppliers', resource: 'suppliers' },
        { id: 'po', icon: 'cart', text: $localize`:@@nav.purchaseOrders:Заказы поставщикам`, path: '/suppliers/purchase-orders', resource: 'purchase-orders' },
        { id: 'customers', icon: 'group', text: $localize`:@@nav.customers:Клиенты`, path: '/customers/customers', resource: 'customers' },
        { id: 'so', icon: 'cart', text: $localize`:@@nav.salesOrders:Заказы клиентов`, path: '/customers/sales-orders', resource: 'sales-orders' },
      ],
    },
    {
      id: 'personnel-group', icon: 'card', module: 'personnel',
      text: $localize`:@@nav.groups.personnel:Персонал`,
      items: [
        { id: 'employees', icon: 'user', text: $localize`:@@nav.employees:Сотрудники`, path: '/personnel/employees', resource: 'employees' },
        { id: 'positions', icon: 'preferences', text: $localize`:@@nav.positions:Должности`, path: '/personnel/positions', resource: 'positions' },
        { id: 'departments', icon: 'group', text: $localize`:@@nav.departments:Отделы`, path: '/personnel/departments', resource: 'departments' },
        { id: 'shifts', icon: 'clock', text: $localize`:@@nav.shifts:Смены`, path: '/personnel/shifts', resource: 'shifts' },
        { id: 'attendance', icon: 'event', text: $localize`:@@nav.attendance:Посещаемость`, path: '/personnel/attendance', resource: 'attendance' },
      ],
    },
    {
      id: 'vehicles-group', icon: 'car', module: 'vehicles',
      text: $localize`:@@nav.groups.vehicles:Автопарк`,
      items: [
        { id: 'vehicles', icon: 'car', text: $localize`:@@nav.vehicles:Транспорт`, path: '/vehicles/list', resource: 'vehicles' },
        { id: 'vehicle-types', icon: 'filter', text: $localize`:@@nav.vehicleTypes:Типы транспорта`, path: '/vehicles/types', resource: 'vehicleTypes' },
        { id: 'maintenance', icon: 'event', text: $localize`:@@nav.maintenance:ТО`, path: '/vehicles/maintenance', resource: 'maintenance' },
        { id: 'insurance', icon: 'fields', text: $localize`:@@nav.insurance:Страховка`, path: '/vehicles/insurance', resource: 'insurance' },
        { id: 'inspections', icon: 'info', text: $localize`:@@nav.inspections:Техосмотр`, path: '/vehicles/inspections', resource: 'inspections' },
      ],
    },
    {
      id: 'dispatching-group', icon: 'map', module: 'dispatching',
      text: $localize`:@@nav.groups.dispatching:Диспетчерская`,
      items: [
        { id: 'disp-routes', icon: 'map', text: $localize`:@@nav.dispatchingRoutes:Маршруты`, path: '/dispatching/routes', resource: 'delivery-routes' },
        { id: 'disp-geofences', icon: 'globe', text: $localize`:@@nav.geofences:Геозоны`, path: '/dispatching/geofences', resource: 'geofences' },
      ],
    },
    { id: 'reports', icon: 'chart', text: $localize`:@@nav.reports:Отчёты`, path: '/reports', resource: 'reports', module: 'reports' },
    { id: 'admin', icon: 'key', text: $localize`:@@nav.admin:Администрирование`, path: '/admin', resource: 'users' },
    { id: 'superadmin', icon: 'key', text: $localize`:@@nav.superadmin:Суперадмин`, path: '/superadmin/tenants', requiresSuperadmin: true },
  ];

  protected readonly items = signal<NavItem[]>([]);

  ngOnInit(): void {
    this.http.get<{ resources: Record<string, ResourcePermissions>; roles: string[]; enabledModules: string[] }>(
      `${AppSettings.apiBaseUrl}/auth/my-permissions`,
    ).subscribe({
      next: (data) => {
        this.perms.setAll(data.resources, data.roles);
        const isAdmin = data.roles.includes('Admin');
        const isSuperadmin = data.roles.includes('Superadmin');
        const enabledModules = data.enabledModules ?? [];
        this.items.set(this.filterItems(this.allItems, data.resources, isAdmin, isSuperadmin, enabledModules));
      },
      error: () => {
        this.items.set(this.allItems);
      },
    });
  }

  /// Recursively filter items: module restriction applies to Admin too (unless Superadmin).
  private filterItems(
    items: NavItem[],
    permissions: Record<string, ResourcePermissions>,
    isAdmin: boolean,
    isSuperadmin: boolean,
    enabledModules: string[],
  ): NavItem[] {
    const result: NavItem[] = [];
    for (const item of items) {
      // Superadmin-only items: visible ONLY to users with the Superadmin role
      if (item.requiresSuperadmin) {
        if (isSuperadmin) result.push(item);
        continue;
      }
      // Module-restricted items: skip if module is set and not enabled (Superadmin bypasses)
      if (item.module && !isSuperadmin && enabledModules.length > 0 && !enabledModules.includes(item.module)) {
        continue;
      }
      if (item.items && item.items.length > 0) {
        const filteredChildren = this.filterItems(item.items, permissions, isAdmin, isSuperadmin, enabledModules);
        if (filteredChildren.length > 0) {
          result.push({ ...item, items: filteredChildren });
        }
      } else if (!item.resource || isAdmin || isSuperadmin || permissions[item.resource]?.canRead === true) {
        result.push(item);
      }
    }
    return result;
  }

  protected onItemClick(e: any): void {
    const item = e.itemData as NavItem;
    if (item?.path) {
      void this.router.navigate([item.path]);
    } else if (item?.items && item.items.length > 0) {
      // Toggle expand/collapse for parent items when clicking anywhere on the item
      const isExpanded = e.node?.expanded;
      if (isExpanded) {
        e.component.collapseItem(e.itemData.id);
      } else {
        e.component.expandItem(e.itemData.id);
      }
    }
  }
}
