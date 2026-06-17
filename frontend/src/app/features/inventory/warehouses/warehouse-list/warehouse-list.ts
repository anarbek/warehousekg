import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { Warehouse } from '../../models/warehouse.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-warehouse-list',
  imports: [ DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink ],
  templateUrl: './warehouse-list.html',
  styleUrl: './warehouse-list.scss',
})
export class WarehouseList {
  private readonly service = inject(InventoryService);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  constructor() { this.load(); }
  private load(): void {
    this.loading.set(true); this.error.set(null);
    this.service.getWarehouses().subscribe({
      next: (data) => { this.warehouses.set(data); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); },
    });
  }
}
