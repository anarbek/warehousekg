import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { StockAdjustment, StockAdjustmentLine, StockAdjustmentReason } from '../models/stock-adjustment.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
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

interface DisplayLine {
  inventoryItemId: string;
  inventoryItemName: string;
  quantityChange: number;
  notes: string | null;
}

@Component({
  selector: 'app-stock-adjustment-detail',
  standalone: true,
  imports: [DatePipe, DxDataGridModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './stock-adjustment-detail.html',
  styleUrl: './stock-adjustment-detail.scss',
})
export class StockAdjustmentDetail implements OnInit {
  private readonly svc = inject(StockAdjustmentsService);
  private readonly inv = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly doc = signal<StockAdjustment | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);
  protected readonly editableLines = signal<DisplayLine[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.err.set(null);
    forkJoin({
      doc: this.svc.getStockAdjustmentById(this.id),
      items: this.inv.getInventoryItems(),
    }).subscribe({
      next: ({ doc: data, items }) => {
        const m = new Map<string, InventoryItem>(items.map((i) => [i.id, i]));
        this.doc.set(data);
        this.editableLines.set(
          data.lines.map((l: StockAdjustmentLine) => ({
            inventoryItemId: l.inventoryItemId,
            inventoryItemName:
              l.inventoryItemName ||
              (m.get(l.inventoryItemId)
                ? `${m.get(l.inventoryItemId)!.sku} — ${m.get(l.inventoryItemId)!.name}`
                : l.inventoryItemId),
            quantityChange: l.quantityChange,
            notes: l.notes ?? null,
          })),
        );
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

  isDraft(): boolean {
    return this.doc()?.status === 'Draft';
  }

  complete(): void {
    this.saving.set(true);
    this.svc.completeStockAdjustment(this.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.err.set($localize`:@@stockOp.completeError:Не удалось завершить операцию`);
        this.saving.set(false);
      },
    });
  }

  cancelOp(): void {
    this.saving.set(true);
    this.svc.cancelStockAdjustment(this.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.err.set($localize`:@@stockOp.cancelError:Не удалось отменить операцию`);
        this.saving.set(false);
      },
    });
  }

  goBack(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }
}
