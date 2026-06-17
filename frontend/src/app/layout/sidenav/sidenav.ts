import { Component, inject, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DxTreeViewModule } from 'devextreme-angular';
import { Router } from '@angular/router';
import { AppSettings } from '../../core/config/app-settings';
import { PermissionsService, ResourcePermissions } from '../../core/services/permissions.service';

interface NavItem {
  id: string;
  text: string;
  icon: string;
  path: string;
  resource?: string;
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
    { id: 'warehouses', icon: 'home', text: $localize`:@@nav.warehouses:Склады`, path: '/inventory/warehouses', resource: 'warehouses' },
    { id: 'items', icon: 'product', text: $localize`:@@nav.inventory:Товары`, path: '/inventory/items', resource: 'inventory-items' },
    { id: 'categories', icon: 'folder', text: $localize`:@@nav.categories:Категории`, path: '/inventory/categories', resource: 'item-categories' },
    { id: 'uoms', icon: 'info', text: $localize`:@@nav.uoms:Ед. измерения`, path: '/inventory/units-of-measure', resource: 'units-of-measure' },
    { id: 'receiving', icon: 'download', text: $localize`:@@nav.receiving:Поступления`, path: '/stock-operations/receiving', resource: 'stock-receipts' },
    { id: 'picking', icon: 'export', text: $localize`:@@nav.picking:Сборка`, path: '/stock-operations/picking', resource: 'pick-orders' },
    { id: 'packing', icon: 'box', text: $localize`:@@nav.packing:Упаковка`, path: '/stock-operations/packing', resource: 'pack-orders' },
    { id: 'transfer', icon: 'movetofolder', text: $localize`:@@nav.transfer:Перемещения`, path: '/stock-operations/transfer', resource: 'stock-transfers' },
    { id: 'suppliers', icon: 'group', text: $localize`:@@nav.suppliers:Поставщики`, path: '/suppliers/suppliers', resource: 'suppliers' },
    { id: 'po', icon: 'cart', text: $localize`:@@nav.purchaseOrders:Заказы поставщикам`, path: '/suppliers/purchase-orders', resource: 'purchase-orders' },
    { id: 'customers', icon: 'user', text: $localize`:@@nav.customers:Клиенты`, path: '/customers/customers', resource: 'customers' },
    { id: 'so', icon: 'money', text: $localize`:@@nav.salesOrders:Заказы клиентов`, path: '/customers/sales-orders', resource: 'sales-orders' },
    { id: 'adjustments', icon: 'preferences', text: $localize`:@@nav.adjustments:Корректировки`, path: '/adjustments/adjustments', resource: 'stock-adjustments' },
    { id: 'audits', icon: 'checklist', text: $localize`:@@nav.audits:Аудиты`, path: '/adjustments/audits', resource: 'stock-audits' },
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
        const p = data.resources;
        this.items.set(this.allItems.filter(item => {
          if (!item.resource) return true; // Dashboard always visible
          return p[item.resource]?.canRead === true;
        }));
      },
      error: () => {
        // On error, show everything (backend will enforce)
        this.items.set(this.allItems);
      },
    });
  }

  protected onItemClick(e: any): void {
    const item = e.itemData as NavItem;
    if (item?.path) {
      void this.router.navigate([item.path]);
    }
  }
}
