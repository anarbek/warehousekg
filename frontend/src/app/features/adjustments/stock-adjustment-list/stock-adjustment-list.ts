import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { StockAdjustmentReason, StockAdjustmentSummary, StockOperationStatus } from '../models/stock-adjustment.model';
import { StockAdjustmentsService } from '../services/stock-adjustments.service';

const REASON_LABELS: Record<StockAdjustmentReason, string> = {
  Correction: $localize`:@@adj.reason.correction:Корректировка`,
  Damage: $localize`:@@adj.reason.damage:Порча`,
  Loss: $localize`:@@adj.reason.loss:Потеря`,
  Theft: $localize`:@@adj.reason.theft:Кража`,
  Found: $localize`:@@adj.reason.found:Найдено`,
  Expired: $localize`:@@adj.reason.expired:Истёк срок`,
  Other: $localize`:@@adj.reason.other:Прочее`,
};

@Component({
  selector: 'app-stock-adjustment-list',
  standalone: true,
  imports: [DatePipe, DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxProgressBarModule, RouterLink],
  templateUrl: './stock-adjustment-list.html',
  styleUrl: './stock-adjustment-list.scss',
})
export class StockAdjustmentList implements OnInit {
  private readonly svc = inject(StockAdjustmentsService);

  protected readonly rawData = signal<StockAdjustmentSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly err = signal<string | null>(null);
  protected readonly selStatus = signal<string>('');
  protected readonly selReason = signal<string>('');

  protected readonly statuses: (StockOperationStatus | '')[] = ['', 'Draft', 'Completed', 'Cancelled'];
  protected readonly reasons: (StockAdjustmentReason | '')[] = [
    '', 'Correction', 'Damage', 'Loss', 'Theft', 'Found', 'Expired', 'Other',
  ];

  get filteredData(): StockAdjustmentSummary[] {
    let data = this.rawData() || [];
    const s = this.selStatus();
    const r = this.selReason();
    if (s) data = data.filter((d) => d.status === s);
    if (r) data = data.filter((d) => d.reason === r);
    return data;
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.err.set(null);
    this.svc.getStockAdjustments().subscribe({
      next: (d) => {
        this.rawData.set(d);
        this.loading.set(false);
      },
      error: () => {
        this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  reasonLabel(reason: StockAdjustmentReason): string {
    return REASON_LABELS[reason] || reason;
  }
}
