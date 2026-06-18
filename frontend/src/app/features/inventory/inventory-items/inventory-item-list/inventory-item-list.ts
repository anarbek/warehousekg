import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxDateBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { Warehouse } from '../../models/warehouse.model';
import { WarehouseStockItem } from '../../../reports/models/report.model';
import { InventoryService } from '../../services/inventory.service';
import { ReportsService } from '../../../reports/services/reports.service';

@Component({
  selector: 'app-inventory-item-list',
  standalone: true,
  imports: [ DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxDateBoxModule, DxProgressBarModule, RouterLink ],
  templateUrl: './inventory-item-list.html',
  styleUrl: './inventory-item-list.scss',
})
export class InventoryItemList {
  private readonly inventory = inject(InventoryService);
  private readonly reports = inject(ReportsService);

  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly stockItems = signal<WarehouseStockItem[]>([]);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly selWarehouseId = signal<string>('');
  protected readonly dateFrom = signal<Date | null>(null);
  protected readonly dateTo = signal<Date | null>(null);

  get isWarehouseMode(): boolean { return !!this.selWarehouseId(); }

  constructor() { this.load(); }

  private load(): void {
    this.loading.set(true); this.error.set(null);
    const whId = this.selWarehouseId();

    forkJoin({
      warehouses: this.inventory.getWarehouses(),
      items: whId ? this.loadWarehouseStock(whId) : this.loadAllItems(),
    }).subscribe({
      next: ({ warehouses, items }) => {
        this.warehouses.set(warehouses);
        if (whId) { this.stockItems.set(items as WarehouseStockItem[]); this.items.set([]); }
        else { this.items.set(items as InventoryItem[]); this.stockItems.set([]); }
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  private loadWarehouseStock(whId: string) {
    const df = this.dateFrom(); const dt = this.dateTo();
    return this.reports.getWarehouseStock(whId,
      df ? `${df.getFullYear()}-${String(df.getMonth()+1).padStart(2,'0')}-${String(df.getDate()).padStart(2,'0')}` : undefined,
      dt ? `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')}` : undefined);
  }

  private loadAllItems() { return this.inventory.getInventoryItems(); }

  onWarehouseChange(): void { this.load(); }
  onDateChange(): void { if (this.isWarehouseMode) this.load(); }
}
