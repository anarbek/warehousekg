import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  route: string;
  icon: string;
  label: string;
}

@Component({
  selector: 'app-sidenav',
  imports: [MatListModule, MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss',
})
export class Sidenav {
  protected readonly items: NavItem[] = [
    { route: '/dashboard', icon: 'dashboard', label: $localize`:@@nav.dashboard:Панель управления` },
    { route: '/inventory/warehouses', icon: 'warehouse', label: $localize`:@@nav.warehouses:Склады` },
    { route: '/inventory/items', icon: 'inventory_2', label: $localize`:@@nav.inventory:Товары` },
    { route: '/inventory/categories', icon: 'category', label: $localize`:@@nav.categories:Категории` },
    { route: '/inventory/units-of-measure', icon: 'straighten', label: $localize`:@@nav.uoms:Ед. измерения` },
    { route: '/stock-operations/receiving', icon: 'move_to_inbox', label: $localize`:@@nav.receiving:Поступления` },
    { route: '/stock-operations/picking', icon: 'outbound', label: $localize`:@@nav.picking:Сборка` },
    { route: '/stock-operations/packing', icon: 'inventory', label: $localize`:@@nav.packing:Упаковка` },
    { route: '/stock-operations/transfer', icon: 'swap_horiz', label: $localize`:@@nav.transfer:Перемещения` },
    { route: '/suppliers/suppliers', icon: 'local_shipping', label: $localize`:@@nav.suppliers:Поставщики` },
    { route: '/suppliers/purchase-orders', icon: 'shopping_cart', label: $localize`:@@nav.purchaseOrders:Заказы поставщикам` },
    { route: '/customers/customers', icon: 'people', label: $localize`:@@nav.customers:Клиенты` },
    { route: '/customers/sales-orders', icon: 'sell', label: $localize`:@@nav.salesOrders:Заказы клиентов` },
    { route: '/adjustments/adjustments', icon: 'tune', label: $localize`:@@nav.adjustments:Корректировки` },
    { route: '/adjustments/audits', icon: 'fact_check', label: $localize`:@@nav.audits:Аудиты` },
    { route: '/reports', icon: 'bar_chart', label: $localize`:@@nav.reports:Отчёты` },
  ];
}
