import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxButtonModule, DxNumberBoxModule, DxTextBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { CreateStockAuditRequest, StockAuditLineInput } from '../models/audit.model';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AuditsService } from '../services/audits.service';
import { PersonnelService } from '../../personnel/services/personnel.service';

interface FormHeader {
  number: string;
  warehouseId: string;
  notes: string;
  reconciledAtUtc: Date|null;
  employeeId: string|null;
}

@Component({
  selector: 'app-audit-form',
  standalone: true,
  imports: [ReactiveFormsModule, DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxButtonModule, DxNumberBoxModule, DxTextBoxModule, DxProgressBarModule],
  templateUrl: './audit-form.html',
  styleUrl: './audit-form.scss',
})
export class AuditForm implements OnInit {
  private readonly svc = inject(AuditsService);
  private readonly inv = inject(InventoryService);
  private readonly personnel = inject(PersonnelService);
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
    notes: '',
    reconciledAtUtc: null,
    employeeId: null,
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
      employees: this.personnel.getEmployees(),
    }).subscribe({
      next: ({ warehouses, items, employees }) => {
        this.warehouses.set(warehouses.filter((w) => w.isActive));
        this.items.set(items.filter((i) => i.isActive));
        this.loading.set(false);
        this.buildHeaderItems(employees);
      },
      error: () => {
        this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  private buildHeaderItems(employees: any[]): void {
    this.headerItems.set([
      {
        dataField: 'number',
        label: { text: $localize`:@@audit.form.number:Номер` },
        isRequired: true,
        editorOptions: { placeholder: 'AUD-0001', stylingMode: 'outlined' },
      },
      {
        dataField: 'warehouseId',
        editorType: 'dxSelectBox',
        label: { text: $localize`:@@audit.form.warehouse:Склад` },
        isRequired: true,
        editorOptions: {
          dataSource: this.warehouses(),
          displayExpr: 'name',
          valueExpr: 'id',
          stylingMode: 'outlined',
        },
      },
      {
        dataField: 'employeeId',
        editorType: 'dxSelectBox',
        label: { text: 'Сотрудник (опц.)' },
        editorOptions: {
          dataSource: employees,
          displayExpr: 'lastName',
          valueExpr: 'id',
          placeholder: 'Не указан',
          stylingMode: 'outlined',
        },
      },
      {
        dataField: 'notes',
        label: { text: $localize`:@@audit.form.notes:Примечание` },
        editorOptions: { stylingMode: 'outlined' },
      },
      {
        dataField: 'reconciledAtUtc',
        editorType: 'dxDateBox',
        label: { text: 'Дата аудита' },
        editorOptions: { type: 'date', displayFormat: 'dd.MM.yyyy', stylingMode: 'outlined' },
      },
    ]);
  }

  private createLine(): FormGroup {
    return this.fb.group({
      inventoryItemId: ['', Validators.required],
      countedQuantity: [1, [Validators.required, Validators.min(1)]],
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
    const lines: StockAuditLineInput[] = rawLines.map((l: any) => ({
      inventoryItemId: l.inventoryItemId!,
      countedQuantity: Number(l.countedQuantity ?? 0),
    }));

    const request: CreateStockAuditRequest = {
      number: this.formData.number,
      warehouseId: this.formData.warehouseId,
      reconciledAtUtc: this.formData.reconciledAtUtc
        ? `${this.formData.reconciledAtUtc.getFullYear()}-${String(this.formData.reconciledAtUtc.getMonth() + 1).padStart(2, '0')}-${String(this.formData.reconciledAtUtc.getDate()).padStart(2, '0')}`
        : null,
      notes: this.formData.notes || null,
      employeeId: this.formData.employeeId || null,
      lines,
    };

    this.svc.createStockAudit(request).subscribe({
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
