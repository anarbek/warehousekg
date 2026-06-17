import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { Supplier as SupplierModel } from '../models/supplier-po.model';
import { SupplierPoService } from '../services/supplier-po.service';

@Component({ selector: 'app-supplier-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './supplier.html', styleUrl: './supplier-list.scss' })
export class SupplierList {
  private readonly s = inject(SupplierPoService);
  protected readonly items = signal<SupplierModel[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getSuppliers().subscribe({ next: (d:any) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
