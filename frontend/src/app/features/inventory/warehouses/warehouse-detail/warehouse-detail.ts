import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxButtonModule, DxDataGridModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Warehouse } from '../../models/warehouse.model';
import { InventoryItem } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-warehouse-detail',
  imports: [ DxButtonModule, DxDataGridModule, DxProgressBarModule, RouterLink ],
  templateUrl: './warehouse-detail.html',
  styleUrl: './warehouse-detail.scss',
})
export class WarehouseDetail {
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly warehouse = signal<Warehouse | null>(null);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  constructor() { this.load(); }
  private load(): void {
    this.loading.set(true);
    forkJoin({
      warehouse: this.service.getWarehouseById(this.id),
      items: this.service.getInventoryItems(),
    }).subscribe({
      next: ({ warehouse, items }) => {
        this.warehouse.set(warehouse);
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }
  protected delete(): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteWarehouse(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
