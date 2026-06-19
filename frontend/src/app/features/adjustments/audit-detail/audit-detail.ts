import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StockAudit } from '../models/audit.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AuditsService } from '../services/audits.service';

interface DisplayLine {
  inventoryItemId: string;
  inventoryItemName: string;
  systemQuantity: number;
  countedQuantity: number;
  variance: number;
}

@Component({
  selector: 'app-audit-detail',
  standalone: true,
  imports: [DatePipe, DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './audit-detail.html',
  styleUrl: './audit-detail.scss',
})
export class AuditDetail implements OnInit {
  private readonly svc = inject(AuditsService);
  private readonly inv = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly aud = signal<StockAudit | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryItem[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      aud: this.svc.getStockAuditById(this.id),
      items: this.inv.getInventoryItems(),
    }).subscribe({
      next: ({ aud, items }) => {
        this.aud.set(aud);
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  goBack(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }

  isDraft(): boolean {
    return this.aud()?.status === 'Draft';
  }

  complete(): void {
    this.saving.set(true);
    this.svc.completeStockAudit(this.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.error.set($localize`:@@stockOp.completeError:Не удалось завершить операцию`);
        this.saving.set(false);
      },
    });
  }

  cancelOp(): void {
    this.saving.set(true);
    this.svc.cancelStockAudit(this.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.error.set($localize`:@@stockOp.cancelError:Не удалось отменить операцию`);
        this.saving.set(false);
      },
    });
  }

  getItemName(id: string): string {
    const it = this.items().find((i) => i.id === id);
    return it ? `${it.sku} — ${it.name}` : id;
  }

  getLines(): DisplayLine[] {
    const a = this.aud();
    if (!a) return [];
    return a.lines.map((l) => ({
      inventoryItemId: l.inventoryItemId,
      inventoryItemName:
        l.inventoryItemName || this.getItemName(l.inventoryItemId),
      systemQuantity: l.systemQuantity,
      countedQuantity: l.countedQuantity,
      variance: l.variance,
    }));
  }

  /** For summary: total across all lines */
  get totalVariance(): number {
    const a = this.aud();
    if (!a) return 0;
    return a.lines.reduce((sum, l) => sum + l.variance, 0);
  }

  /** Net change if audit is completed (counted - system) */
  get netChange(): number {
    return this.totalVariance;
  }

  delete(): void {
    if (!confirm($localize`:@@stockOp.confirmDelete:Удалить этот аудит безвозвратно?`)) return;
    this.saving.set(true);
    this.svc.deleteStockAudit(this.id).subscribe({
      next: () => {
        this.saving.set(false);
        void this.router.navigate(['..'], { relativeTo: this.route });
      },
      error: () => {
        this.error.set($localize`:@@stockOp.deleteError:Не удалось удалить аудит`);
        this.saving.set(false);
      },
    });
  }

  canDelete(): boolean {
    const status = this.aud()?.status;
    // Allow deletion of Draft, Cancelled, or Completed audits
    return status === 'Draft' || status === 'Cancelled' || status === 'Completed';
  }
}