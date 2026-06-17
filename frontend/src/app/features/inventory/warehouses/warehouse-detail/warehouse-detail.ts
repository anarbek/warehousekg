import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxButtonModule, DxDataGridModule, DxDateBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Warehouse } from '../../models/warehouse.model';
import { WarehouseStockItem } from '../../../reports/models/report.model';
import { InventoryService } from '../../services/inventory.service';
import { ReportsService } from '../../../reports/services/reports.service';

@Component({
  selector: 'app-warehouse-detail',
  imports: [ DxButtonModule, DxDataGridModule, DxDateBoxModule, DxProgressBarModule, RouterLink ],
  templateUrl: './warehouse-detail.html',
  styleUrl: './warehouse-detail.scss',
})
export class WarehouseDetail {
  private readonly service = inject(InventoryService);
  private readonly reports = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly warehouse = signal<Warehouse | null>(null);
  protected readonly stockItems = signal<WarehouseStockItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly dateFrom = signal<Date | null>(null);
  protected readonly dateTo = signal<Date | null>(null);

  constructor() { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    const df = this.dateFrom();
    const dt = this.dateTo();
    const dateFromStr = df ? df.toISOString() : undefined;
    const dateToStr = dt ? new Date(dt.getFullYear(), dt.getMonth(), dt.getDate(), 23, 59, 59).toISOString() : undefined;

    forkJoin({
      warehouse: this.service.getWarehouseById(this.id),
      stock: this.reports.getWarehouseStock(this.id, dateFromStr, dateToStr),
    }).subscribe({
      next: ({ warehouse, stock }) => {
        this.warehouse.set(warehouse);
        this.stockItems.set(stock);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  onDateChange(): void {
    this.load();
  }

  protected delete(): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteWarehouse(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
