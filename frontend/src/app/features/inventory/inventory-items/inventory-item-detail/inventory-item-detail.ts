import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { DxButtonModule, DxDataGridModule, DxSelectBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { Warehouse } from '../../models/warehouse.model';
import { ItemMovement } from '../../../reports/models/report.model';
import { InventoryService } from '../../services/inventory.service';
import { ReportsService } from '../../../reports/services/reports.service';

@Component({
  selector: 'app-inventory-item-detail',
  standalone: true,
  imports: [DatePipe, DxButtonModule, DxDataGridModule, DxSelectBoxModule, DxProgressBarModule, RouterLink],
  templateUrl: './inventory-item-detail.html',
  styleUrl: './inventory-item-detail.scss',
})
export class InventoryItemDetail {
  private readonly service = inject(InventoryService);
  private readonly reports = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly item = signal<InventoryItem | null>(null);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly movements = signal<ItemMovement[]>([]);
  protected readonly selWarehouseId = signal<string>('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    // Pre-select warehouse from query param
    const whParam = this.route.snapshot.queryParamMap.get('warehouseId');
    if (whParam) this.selWarehouseId.set(whParam);
    this.load();
  }

  private load(): void {
    this.loading.set(true); this.error.set(null);
    const whId = this.selWarehouseId();

    const movements$ = whId
      ? this.reports.getItemMovements(this.id, whId)
      : of([] as ItemMovement[]);

    forkJoin({
      item: this.service.getInventoryItemById(this.id),
      warehouses: this.service.getWarehouses(),
      movements: movements$,
    }).subscribe({
      next: ({ item, warehouses, movements }) => {
        this.item.set(item);
        this.warehouses.set(warehouses);
        this.movements.set(movements);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  onWarehouseChange(): void { this.load(); }

  getDocumentRoute(m: ItemMovement): string[] | null {
    const op = m.operationType;
    const id = m.documentId;
    if (op.startsWith('Поступление')) return ['/stock-operations/receiving', id];
    if (op.startsWith('Корректировка')) return ['/adjustments/adjustments', id];
    if (op.startsWith('Аудит')) return ['/adjustments/audits', id];
    if (op.startsWith('Перемещение')) return ['/stock-operations/transfer', id];
    if (op.startsWith('Сборка')) return ['/stock-operations/picking', id];
    if (op.startsWith('Закупка')) return ['/suppliers/purchase-orders', id];
    if (op.startsWith('Продажа')) return ['/customers/sales-orders', id];
    return null;
  }

  get runningBalance(): number {
    const m = this.movements();
    return m.length > 0 ? m[m.length - 1].runningBalance : 0;
  }

  protected delete(): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteInventoryItem(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
