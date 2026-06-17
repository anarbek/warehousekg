import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { PurchaseOrderSummary } from '../../models/supplier-po.model';
import { SupplierPoService } from '../../services/supplier-po.service';

@Component({ selector: 'app-po-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './po-list.html', styleUrl: './po-list.scss' })
export class PoList {
  private readonly s = inject(SupplierPoService);
  protected readonly items = signal<PurchaseOrderSummary[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getPurchaseOrders().subscribe({ next: (d:any) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
