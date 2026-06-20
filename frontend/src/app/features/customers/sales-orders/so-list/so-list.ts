import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { SalesOrderSummary } from '../../models/customer-so.model';
import { CustomerSoService } from '../../services/customer-so.service';
import { PermissionsService } from '../../../../core/services/permissions.service';

@Component({ selector: 'app-so-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink,DatePipe], templateUrl: './so-list.html', styleUrl: './so-list.scss' })
export class SoList {
  private readonly s = inject(CustomerSoService);
  readonly perms = inject(PermissionsService);
  protected readonly items = signal<SalesOrderSummary[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  readonly canDelete = this.perms.canDelete('sales-orders');
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getSalesOrders().subscribe({ next: (d:any) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
  protected async deleteSo(id: string) {
    if (!confirm('Удалить заказ?')) return;
    try {
      await firstValueFrom(this.s.deleteSalesOrder(id));
      this.load();
    } catch { }
  }
}
