import { Component, inject } from '@angular/core';
import { DxTreeViewModule } from 'devextreme-angular';
import { Router } from '@angular/router';

interface NavItem {
  id: string;
  text: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-sidenav',
  imports: [DxTreeViewModule],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss',
})
export class Sidenav {
  private readonly router = inject(Router);

  protected readonly items: NavItem[] = [
    { id: 'dashboard', icon: 'chart', text: $localize`:@@nav.dashboard:Панель управления`, path: '/dashboard' },
    { id: 'warehouses', icon: 'home', text: $localize`:@@nav.warehouses:Склады`, path: '/inventory/warehouses' },
    { id: 'items', icon: 'product', text: $localize`:@@nav.inventory:Товары`, path: '/inventory/items' },
    { id: 'categories', icon: 'folder', text: $localize`:@@nav.categories:Категории`, path: '/inventory/categories' },
    { id: 'uoms', icon: 'info', text: $localize`:@@nav.uoms:Ед. измерения`, path: '/inventory/units-of-measure' },
    { id: 'receiving', icon: 'download', text: $localize`:@@nav.receiving:Поступления`, path: '/stock-operations/receiving' },
    { id: 'picking', icon: 'export', text: $localize`:@@nav.picking:Сборка`, path: '/stock-operations/picking' },
    { id: 'packing', icon: 'box', text: $localize`:@@nav.packing:Упаковка`, path: '/stock-operations/packing' },
    { id: 'transfer', icon: 'movetofolder', text: $localize`:@@nav.transfer:Перемещения`, path: '/stock-operations/transfer' },
    { id: 'suppliers', icon: 'group', text: $localize`:@@nav.suppliers:Поставщики`, path: '/suppliers/suppliers' },
    { id: 'po', icon: 'cart', text: $localize`:@@nav.purchaseOrders:Заказы поставщикам`, path: '/suppliers/purchase-orders' },
    { id: 'customers', icon: 'user', text: $localize`:@@nav.customers:Клиенты`, path: '/customers/customers' },
    { id: 'so', icon: 'money', text: $localize`:@@nav.salesOrders:Заказы клиентов`, path: '/customers/sales-orders' },
    { id: 'adjustments', icon: 'preferences', text: $localize`:@@nav.adjustments:Корректировки`, path: '/adjustments/adjustments' },
    { id: 'audits', icon: 'checklist', text: $localize`:@@nav.audits:Аудиты`, path: '/adjustments/audits' },
    { id: 'reports', icon: 'chart', text: $localize`:@@nav.reports:Отчёты`, path: '/reports' },
    { id: 'admin', icon: 'key', text: $localize`:@@nav.admin:Администрирование`, path: '/admin' },
  ];

  protected onItemClick(e: any): void {
    const item = e.itemData as NavItem;
    if (item?.path) {
      void this.router.navigate([item.path]);
    }
  }
}
