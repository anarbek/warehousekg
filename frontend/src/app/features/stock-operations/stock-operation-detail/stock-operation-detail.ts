import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import {
  OperationType,
  StockOperationLine,
  StockReceipt,
  PickOrder,
  PackOrder,
  StockTransfer,
} from '../models/stock-operation.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockOperationsService } from '../services/stock-operations.service';

type Doc = StockReceipt | PickOrder | PackOrder | StockTransfer;

@Component({
  selector: 'app-stock-operation-detail',
  imports: [
    FormsModule,
    DatePipe,
    LowerCasePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './stock-operation-detail.html',
  styleUrl: './stock-operation-detail.scss',
})
export class StockOperationDetail implements OnInit {
  private readonly service = inject(StockOperationsService);
  private readonly inventoryService = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly operationType = this.route.snapshot.data['operationType'] as OperationType;
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly doc = signal<Doc | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly lineColumns = [
    'inventoryItemName',
    'quantity',
    'warehouseLocationName',
    'packageLabel',
    'actions',
  ];

  /** Deep copy of lines for inline editing. */
  protected editableLines = signal<EditableLine[]>([]);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    const op = this.operationType;
    const docObs = (
      op === 'receiving'
        ? this.service.getStockReceiptById(this.id)
        : op === 'picking'
          ? this.service.getPickOrderById(this.id)
          : op === 'packing'
            ? this.service.getPackOrderById(this.id)
            : this.service.getStockTransferById(this.id)
    ) as Observable<Doc>;

    forkJoin({ doc: docObs, items: this.inventoryService.getInventoryItems() })
    .subscribe({
      next: ({ doc: data, items }) => {
        const itemMap = new Map(items.map(i => [i.id, i]));
        this.doc.set(data);
        this.editableLines.set(data.lines.map((l: StockOperationLine) => {
          const item = itemMap.get(l.inventoryItemId);
          return {
            inventoryItemId: l.inventoryItemId,
            inventoryItemName: item ? `${item.sku} — ${item.name}` : l.inventoryItemName ?? l.inventoryItemId,
            quantity: l.quantity,
            warehouseLocationName: l.warehouseLocationName ?? (l.warehouseLocationId ?? ''),
            packageLabel: l.packageLabel ?? '',
          };
        }));
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected removeLine(index: number): void {
    this.editableLines.update(lines => { lines.splice(index, 1); return [...lines]; });
  }

  protected addLine(): void {
    this.editableLines.update(lines => [...lines, {
      inventoryItemId: '',
      inventoryItemName: '',
      quantity: 0,
      warehouseLocationName: '',
      packageLabel: '',
    }]);
  }

  protected isDraft(): boolean {
    return this.doc()?.status === 'Draft';
  }

  protected complete(): void {
    this.statusAction('complete');
  }

  protected cancel(): void {
    this.statusAction('cancel');
  }

  private statusAction(action: 'complete' | 'cancel'): void {
    const op = this.operationType;
    const doc = this.doc();
    if (!doc) return;
    this.saving.set(true);

    const obs = (
      op === 'receiving'
        ? (action === 'complete'
            ? this.service.completeStockReceipt(this.id)
            : this.service.cancelStockReceipt(this.id))
        : op === 'picking'
          ? (action === 'complete'
              ? this.service.completePickOrder(this.id)
              : this.service.cancelPickOrder(this.id))
          : op === 'packing'
            ? (action === 'complete'
                ? this.service.completePackOrder(this.id)
                : this.service.cancelPackOrder(this.id))
            : (action === 'complete'
                ? this.service.completeStockTransfer(this.id)
                : this.service.cancelStockTransfer(this.id))
    ) as Observable<void>;

    obs.subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.error.set(
          action === 'complete'
            ? $localize`:@@stockOp.completeError:Не удалось завершить операцию`
            : $localize`:@@stockOp.cancelError:Не удалось отменить операцию`,
        );
        this.saving.set(false);
      },
    });
  }

  protected goBack(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }

  /** Title prefix based on operation type. */
  protected get titleKey(): string {
    const map: Record<OperationType, string> = {
      receiving: $localize`:@@stockOp.receiving.detail.title:Поступление`,
      picking: $localize`:@@stockOp.picking.detail.title:Заказ на сборку`,
      packing: $localize`:@@stockOp.packing.detail.title:Заказ на упаковку`,
      transfer: $localize`:@@stockOp.transfer.detail.title:Перемещение`,
    };
    return map[this.operationType];
  }

  protected get warehouseInfo(): string {
    const d = this.doc();
    if (!d) return '';
    if ('sourceWarehouseName' in d && d.sourceWarehouseName) {
      return `${d.sourceWarehouseName} → ${d.destinationWarehouseName ?? '—'}`;
    }
    return (d as any).warehouseName ?? '—';
  }

  protected get referenceInfo(): string | null {
    const d = this.doc();
    if (!d) return null;
    if ('supplierReference' in d && d.supplierReference) return d.supplierReference;
    if ('reference' in d && d.reference) return d.reference;
    return null;
  }

  protected get pickOrderIdInfo(): string | null {
    const d = this.doc();
    if (!d) return null;
    if ('pickOrderId' in d && (d as PackOrder).pickOrderId) return (d as PackOrder).pickOrderId!;
    return null;
  }
}

interface EditableLine {
  inventoryItemId: string;
  inventoryItemName: string;
  quantity: number;
  warehouseLocationName: string;
  packageLabel: string;
}
