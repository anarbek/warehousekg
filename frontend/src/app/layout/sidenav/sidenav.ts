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
      id: 'warehouse-group', icon: 'home',
      text: $localize`:@@nav.groups.warehouse:Склад`,
      expanded: true,
      items: [
        { id: 'warehouses', text: $localize`:@@nav.warehouses:Склады`, path: '/inventory/warehouses', resource: 'warehouses' },
        { id: 'items', text: $localize`:@@nav.inventory:Товары`, path: '/inventory/items', resource: 'inventory-items' },
        { id: 'categories', text: $localize`:@@nav.categories:Категории`, path: '/inventory/categories', resource: 'item-categories' },
        { id: 'uoms', text: $localize`:@@nav.uoms:Ед. измерения`, path: '/inventory/units-of-measure', resource: 'units-of-measure' },
        { id: 'receiving', text: $localize`:@@nav.receiving:Поступления`, path: '/stock-operations/receiving', resource: 'stock-receipts' },
        { id: 'picking', text: $localize`:@@nav.picking:Сборка`, path: '/stock-operations/picking', resource: 'pick-orders' },
        { id: 'packing', text: $localize`:@@nav.packing:Упаковка`, path: '/stock-operations/packing', resource: 'pack-orders' },
        { id: 'transfer', text: $localize`:@@nav.transfer:Перемещения`, path: '/stock-operations/transfer', resource: 'stock-transfers' },
        { id: 'adjustments', text: $localize`:@@nav.adjustments:Корректировки`, path: '/adjustments/adjustments', resource: 'stock-adjustments' },
        { id: 'audits', text: $localize`:@@nav.audits:Аудиты`, path: '/adjustments/audits', resource: 'stock-audits' },
      ],
    },
    {
      id: 'crm-group', icon: 'user',
      text: $localize`:@@nav.groups.crm:Клиенты и поставщики`,
      items: [
        { id: 'suppliers', text: $localize`:@@nav.suppliers:Поставщики`, path: '/suppliers/suppliers', resource: 'suppliers' },
        { id: 'po', text: $localize`:@@nav.purchaseOrders:Заказы поставщикам`, path: '/suppliers/purchase-orders', resource: 'purchase-orders' },
        { id: 'customers', text: $localize`:@@nav.customers:Клиенты`, path: '/customers/customers', resource: 'customers' },
        { id: 'so', text: $localize`:@@nav.salesOrders:Заказы клиентов`, path: '/customers/sales-orders', resource: 'sales-orders' },
      ],
    },
    {
      id: 'personnel-group', icon: 'card',
      text: $localize`:@@nav.groups.personnel:Персонал`,
      items: [
        { id: 'employees', text: $localize`:@@nav.employees:Сотрудники`, path: '/personnel/employees', resource: 'employees' },
        { id: 'positions', text: $localize`:@@nav.positions:Должности`, path: '/personnel/positions', resource: 'positions' },
        { id: 'departments', text: $localize`:@@nav.departments:Отделы`, path: '/personnel/departments', resource: 'departments' },
        { id: 'shifts', text: $localize`:@@nav.shifts:Смены`, path: '/personnel/shifts', resource: 'shifts' },
        { id: 'attendance', text: $localize`:@@nav.attendance:Посещаемость`, path: '/personnel/attendance', resource: 'attendance' },
      ],
    },
    {
      id: 'vehicles-group', icon: 'car',
      text: $localize`:@@nav.groups.vehicles:Автопарк`,
      items: [
        { id: 'vehicles', text: $localize`:@@nav.vehicles:Транспорт`, path: '/vehicles/list', resource: 'vehicles' },
        { id: 'vehicle-types', text: $localize`:@@nav.vehicleTypes:Типы транспорта`, path: '/vehicles/types', resource: 'vehicleTypes' },
        { id: 'maintenance', text: $localize`:@@nav.maintenance:ТО`, path: '/vehicles/maintenance', resource: 'maintenance' },
        { id: 'insurance', text: $localize`:@@nav.insurance:Страховка`, path: '/vehicles/insurance', resource: 'insurance' },
        { id: 'inspections', text: $localize`:@@nav.inspections:Техосмотр`, path: '/vehicles/inspections', resource: 'inspections' },
      ],
    },
    { id: 'reports', icon: 'chart', text: $localize`:@@nav.reports:Отчёты`, path: '/reports', resource: 'reports' },
    { id: 'admin', icon: 'key', text: $localize`:@@nav.admin:Администрирование`, path: '/admin', resource: 'users' },
  ];

  protected readonly items = signal<NavItem[]>([]);

  ngOnInit(): void {
    this.http.get<{ resources: Record<string, ResourcePermissions>; roles: string[] }>(
      `${AppSettings.apiBaseUrl}/auth/my-permissions`,
    ).subscribe({
      next: (data) => {
        this.perms.setAll(data.resources, data.roles);
        const isAdmin = data.roles.includes('Admin');
        this.items.set(this.filterItems(this.allItems, data.resources, isAdmin));
      },
      error: () => {
        this.items.set(this.allItems);
      },
    });
  }

  /// Recursively filter items: Admin sees everything, others by permission.
  private filterItems(items: NavItem[], permissions: Record<string, ResourcePermissions>, isAdmin: boolean): NavItem[] {
    const result: NavItem[] = [];
    for (const item of items) {
      if (item.items && item.items.length > 0) {
        const filteredChildren = this.filterItems(item.items, permissions, isAdmin);
        if (filteredChildren.length > 0) {
          result.push({ ...item, items: filteredChildren });
        }
      } else if (!item.resource || isAdmin || permissions[item.resource]?.canRead === true) {
        result.push(item);
      }
    }
    return result;
  }

  protected onItemClick(e: any): void {
    const item = e.itemData as NavItem;
    if (item?.path) {
      void this.router.navigate([item.path]);
    }
  }
}
