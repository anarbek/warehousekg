import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxDateBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { StockAuditSummary, StockOperationStatus } from '../models/audit.model';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AuditsService } from '../services/audits.service';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [DatePipe, DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxDateBoxModule, DxProgressBarModule, RouterLink],
  templateUrl: './audit-list.html',
  styleUrl: './audit-list.scss',
})
export class AuditList implements OnInit {
  private readonly svc = inject(AuditsService);
  private readonly inv = inject(InventoryService);

  protected readonly rawData = signal<StockAuditSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly err = signal<string | null>(null);

  // Filters
  protected readonly selStatus = signal<string>('');
  protected readonly selWarehouseId = signal<string>('');
  protected readonly dateFrom = signal<Date | null>(null);
  protected readonly dateTo = signal<Date | null>(null);

  // Reference data
  protected readonly warehouses = signal<Warehouse[]>([]);

  protected readonly statuses: (StockOperationStatus | '')[] = ['', 'Draft', 'Completed', 'Cancelled'];

  readonly filteredData = computed(() => {
    let data = this.rawData() || [];
    const s = this.selStatus();
    const wh = this.selWarehouseId();
    const from = this.dateFrom();
    const to = this.dateTo();

    if (s) data = data.filter((d) => d.status === s);
    if (wh) data = data.filter((d) => d.warehouseId === wh);
    if (from) {
      const fromTs = from.getTime();
      data = data.filter((d) => {
        const dt = d.reconciledAtUtc ? new Date(d.reconciledAtUtc).getTime() : 0;
        return dt >= fromTs;
      });
    }
    if (to) {
      const toTs = to.getTime() + 86400000; // end of day
      data = data.filter((d) => {
        const dt = d.reconciledAtUtc ? new Date(d.reconciledAtUtc).getTime() : 0;
        return dt <= toTs;
      });
    }
    return data;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.err.set(null);
    forkJoin({
      audits: this.svc.getStockAudits(),
      warehouses: this.inv.getWarehouses(),
    }).subscribe({
      next: ({ audits, warehouses }) => {
        this.rawData.set(audits);
        this.warehouses.set(warehouses);
        this.loading.set(false);
      },
      error: () => {
        this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  warehouseName(id: string): string {
    return this.warehouses().find((w) => w.id === id)?.name || '—';
  }
}
