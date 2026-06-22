import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin, firstValueFrom } from 'rxjs';
import { DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InvoiceService } from '../services/invoice.service';
import { InventoryService } from '../../inventory/services/inventory.service';
import { CustomerSoService } from '../../customers/services/customer-so.service';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { Customer } from '../../customers/models/customer-so.model';
import { ErrorToastService } from '../../../core/services/error-toast.service';

const PAYMENT_TYPES = ['Наличные', 'Безналичные', 'Карта', 'Перевод'];
const CURRENCIES = ['KGS', 'USD', 'EUR', 'RUB', 'KZT', 'CNY', 'TRY'];

@Component({
  selector: 'app-invoice-form',
  imports: [ReactiveFormsModule, DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule, RouterLink, DecimalPipe],
  templateUrl: './invoice-form.html',
  styleUrl: './invoice-form.scss',
})
export class InvoiceForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(InvoiceService);
  private readonly inv = inject(InventoryService);
  private readonly cust = inject(CustomerSoService);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);
  private readonly route = inject(ActivatedRoute);

  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);
  protected readonly ready = signal(false);
  protected readonly editId = signal<string | null>(null);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly customers = signal<Customer[]>([]);
  protected readonly headerItems = signal<any[]>([]);

  protected formData: any = {
    customerId: '',
    warehouseId: '',
    currency: 'KGS',
    exchangeRate: 1,
    paymentType: '',
    dueDateUtc: null,
    notes: '',
  };

  protected readonly form = this.fb.group({
    lines: this.fb.array([this.cl()]),
  });

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  private cl() {
    return this.fb.group({
      inventoryItemId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      taxRate: [0],
    });
  }

  protected lineTotals = signal<{ lineTotal: number; taxAmount: number; rowTotal: number }[]>([]);
  protected subtotal = signal(0);
  protected taxTotal = signal(0);
  protected grandTotal = signal(0);
  protected totalQty = signal(0);

  private recalcTotals() {
    const totals = this.lines.controls.map((l) => {
      const qty = Number(l.get('quantity')?.value ?? 0);
      const price = Number(l.get('unitPrice')?.value ?? 0);
      const tax = Number(l.get('taxRate')?.value ?? 0);
      const lineTotal = qty * price;
      const taxAmount = lineTotal * tax / 100;
      return { lineTotal, taxAmount, rowTotal: lineTotal + taxAmount };
    });
    this.lineTotals.set(totals);
    this.subtotal.set(totals.reduce((s, t) => s + t.lineTotal, 0));
    this.taxTotal.set(totals.reduce((s, t) => s + t.taxAmount, 0));
    this.grandTotal.set(this.subtotal() + this.taxTotal());
    this.totalQty.set(this.lines.controls.reduce((s, l) => s + Number(l.get('quantity')?.value ?? 0), 0));
  }

  ngOnInit() {
    // React to any form value change for live recalculation
    this.form.valueChanges.subscribe(() => this.recalcTotals());

    const id = this.route.snapshot.paramMap.get('id');
    this.editId.set(id || null);
    forkJoin({
      warehouses: this.inv.getWarehouses(),
      items: this.inv.getInventoryItems(),
      customers: this.cust.getCustomers(),
    }).subscribe({
      next: async (d) => {
        this.warehouses.set(d.warehouses);
        this.items.set(d.items);
        this.customers.set(d.customers);

        this.headerItems.set([
          { dataField: 'customerId', editorType: 'dxSelectBox', label: { text: 'Клиент' }, isRequired: true,
            editorOptions: { dataSource: d.customers, displayExpr: 'name', valueExpr: 'id', stylingMode: 'outlined' } },
          { dataField: 'warehouseId', editorType: 'dxSelectBox', label: { text: 'Склад' }, isRequired: true,
            editorOptions: { dataSource: d.warehouses, displayExpr: 'name', valueExpr: 'id', stylingMode: 'outlined' } },
          { dataField: 'currency', editorType: 'dxSelectBox', label: { text: 'Валюта' },
            editorOptions: { dataSource: CURRENCIES, stylingMode: 'outlined' } },
          { dataField: 'exchangeRate', label: { text: 'Курс' }, editorOptions: { placeholder: '1', stylingMode: 'outlined' } },
          { dataField: 'paymentType', editorType: 'dxSelectBox', label: { text: 'Тип оплаты' },
            editorOptions: { dataSource: PAYMENT_TYPES, stylingMode: 'outlined', placeholder: 'Выберите...' } },
          { dataField: 'dueDateUtc', editorType: 'dxDateBox', label: { text: 'Срок оплаты' },
            editorOptions: { type: 'date', displayFormat: 'dd.MM.yyyy', stylingMode: 'outlined' } },
          { dataField: 'notes', label: { text: 'Примечание' }, editorOptions: { stylingMode: 'outlined' } },
        ]);

        // Load existing invoice for edit mode
        if (id) {
          try {
            const inv = await firstValueFrom(this.svc.getInvoiceById(id));
            this.formData.customerId = inv.customerId;
            this.formData.warehouseId = inv.warehouseId;
            this.formData.currency = inv.currency;
            this.formData.exchangeRate = inv.exchangeRate;
            this.formData.paymentType = inv.paymentType ?? '';
            this.formData.dueDateUtc = inv.dueDateUtc ?? null;
            this.formData.notes = inv.notes ?? '';
            if (inv.lines && inv.lines.length > 0) {
              this.lines.clear();
              for (const l of inv.lines) {
                this.lines.push(this.fb.group({
                  inventoryItemId: [l.inventoryItemId, Validators.required],
                  quantity: [l.quantity, [Validators.required, Validators.min(1)]],
                  unitPrice: [l.unitPrice, [Validators.required, Validators.min(0)]],
                  taxRate: [l.taxRate],
                }));
              }
            }
          } catch { /* ignore */ }
        }

        this.recalcTotals();
        this.ready.set(true);
      },
    });
  }

  /** When user selects an inventory item, auto-fill its unit price */
  onItemSelected(e: any, index: number) {
    const itemId = e.value;
    if (!itemId) return;
    const item = this.items().find((i) => i.id === itemId);
    if (item && (item.unitPrice ?? 0) > 0) {
      const line = this.lines.at(index);
      line.patchValue({ unitPrice: item.unitPrice });
    }
  }

  /** Select all text on focus for fast editing */
  onTextBoxFocus(e: any) {
    setTimeout(() => {
      const input = e.element?.querySelector?.('input');
      if (input) input.select();
    });
  }

  addLine() {
    this.lines.push(this.cl());
    setTimeout(() => this.recalcTotals());
  }

  removeLine(i: number) {
    if (this.lines.length > 1) {
      this.lines.removeAt(i);
      setTimeout(() => this.recalcTotals());
    }
  }

  submit() {
    this.saving.set(true);
    this.err.set(null);

    const body = {
      customerId: this.formData.customerId,
      warehouseId: this.formData.warehouseId,
      currency: this.formData.currency || 'KGS',
      exchangeRate: Number(this.formData.exchangeRate) || 1,
      paymentType: this.formData.paymentType || null,
      dueDateUtc: this.formData.dueDateUtc ? new Date(this.formData.dueDateUtc).toISOString() : null,
      notes: this.formData.notes || null,
      lines: this.form.getRawValue().lines
        .filter((l: any) => l.inventoryItemId)
        .map((l: any) => ({
        inventoryItemId: l.inventoryItemId!,
        quantity: Number(l.quantity),
        unitPrice: Number(l.unitPrice),
        taxRate: Number(l.taxRate) || 0,
      })),
    };

    const id = this.editId();
    if (id) {
      this.svc.updateInvoice(id, body).subscribe({
        next: () => { this.saving.set(false); void this.router.navigate(['/invoices', id]); },
        error: (e: any) => { this.toast.showSave(e); this.saving.set(false); },
      });
    } else {
      this.svc.createInvoice(body).subscribe({
        next: (newId: string) => { this.saving.set(false); void this.router.navigate(['/invoices', newId]); },
        error: (e: any) => { this.toast.showSave(e); this.saving.set(false); },
      });
    }
  }

  cancel() {
    const id = this.editId();
    void this.router.navigate(id ? ['/invoices', id] : ['/invoices/list']);
  }
}
