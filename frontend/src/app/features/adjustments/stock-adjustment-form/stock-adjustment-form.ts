import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxButtonModule, DxNumberBoxModule, DxTextBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { StockAdjustmentReason, CreateStockAdjustmentRequest, StockAdjustmentLineInput } from '../models/stock-adjustment.model';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockAdjustmentsService } from '../services/stock-adjustments.service';

interface FormHeader {
  number: string;
  warehouseId: string;
  reason: StockAdjustmentReason;
  notes: string;
  adjustedAtUtc: Date|null;
}

const REASONS: StockAdjustmentReason[] = ['Correction', 'Damage', 'Loss', 'Theft', 'Found', 'Expired', 'Other'];

@Component({
  selector: 'app-stock-adjustment-form',
  standalone: true,
  imports: [ReactiveFormsModule, DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxButtonModule, DxNumberBoxModule, DxTextBoxModule, DxProgressBarModule],
  templateUrl: './stock-adjustment-form.html',
  styleUrl: './stock-adjustment-form.scss',
})
export class StockAdjustmentForm implements OnInit {
  private readonly svc = inject(StockAdjustmentsService);
  private readonly inv = inject(InventoryService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);

  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly items = signal<InventoryItem[]>([]);

  protected formData: FormHeader = {
    number: '',
    warehouseId: '',
    reason: 'Correction',
    notes: '',
    adjustedAtUtc: null,
  };

  protected readonly form = this.fb.group({
    lines: this.fb.array([this.createLine()]),
  });

  get linesArray(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  protected readonly headerItems = signal<any[]>([]);

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      warehouses: this.inv.getWarehouses(),
      items: this.inv.getInventoryItems(),
    }).subscribe({
      next: ({ warehouses, items }) => {
        this.warehouses.set(warehouses.filter((w) => w.isActive));
        this.items.set(items.filter((i) => i.isActive));
        this.loading.set(false);
        this.buildHeaderItems();
      },
      error: () => {
        this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  private buildHeaderItems(): void {
    this.headerItems.set([
      {
        dataField: 'number',
        label: { text: $localize`:@@adj.form.number:Номер` },
        isRequired: true,
        editorOptions: { placeholder: 'ADJ-0001', stylingMode: 'outlined' },
      },
      {
        dataField: 'warehouseId',
        editorType: 'dxSelectBox',
        label: { text: $localize`:@@adj.form.warehouse:Склад` },
        isRequired: true,
        editorOptions: {
          dataSource: this.warehouses(),
          displayExpr: 'name',
          valueExpr: 'id',
          stylingMode: 'outlined',
        },
      },
      {
        dataField: 'reason',
        editorType: 'dxSelectBox',
        label: { text: $localize`:@@adj.form.reason:Причина` },
        isRequired: true,
        editorOptions: {
          dataSource: REASONS,
          stylingMode: 'outlined',
        },
      },
      {
        dataField: 'notes',
        label: { text: $localize`:@@adj.form.notes:Примечание` },
        editorOptions: { stylingMode: 'outlined' },
      },
      {
        dataField: 'adjustedAtUtc',
        editorType: 'dxDateBox',
        label: { text: 'Дата корректировки' },
        editorOptions: { type: 'date', displayFormat: 'dd.MM.yyyy', stylingMode: 'outlined' },
      },
    ]);
  }

  private createLine(): FormGroup {
    return this.fb.group({
      inventoryItemId: ['', Validators.required],
      quantityChange: [0, Validators.required],
      notes: [''],
    });
  }

  addLine(): void {
    this.linesArray.push(this.createLine());
  }

  removeLine(index: number): void {
    if (this.linesArray.length > 1) {
      this.linesArray.removeAt(index);
    }
  }

  submit(): void {
    if (!this.formData.number || !this.formData.warehouseId || this.linesArray.invalid) {
      return;
    }

    this.saving.set(true);
    this.err.set(null);

    const rawLines: any[] = this.form.getRawValue().lines;
    const lines: StockAdjustmentLineInput[] = rawLines.map((l: any) => ({
      inventoryItemId: l.inventoryItemId!,
      quantityChange: Number(l.quantityChange ?? 0),
      notes: l.notes || null,
    }));

    const request: CreateStockAdjustmentRequest = {
      number: this.formData.number,
      warehouseId: this.formData.warehouseId,
      reason: this.formData.reason,
      adjustedAtUtc: this.formData.adjustedAtUtc
        ? `${this.formData.adjustedAtUtc.getFullYear()}-${String(this.formData.adjustedAtUtc.getMonth() + 1).padStart(2, '0')}-${String(this.formData.adjustedAtUtc.getDate()).padStart(2, '0')}`
        : null,
      notes: this.formData.notes || null,
      lines,
    };

    this.svc.createStockAdjustment(request).subscribe({
      next: (id) => {
        this.saving.set(false);
        void this.router.navigate(['..', id], { replaceUrl: true });
      },
      error: () => {
        this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);
        this.saving.set(false);
      },
    });
  }

  goBack(): void {
    void this.router.navigate(['..']);
  }

}
