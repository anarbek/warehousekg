import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { DxFormModule, DxButtonModule, DxProgressBarModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxNumberBoxModule, DxDataGridModule } from 'devextreme-angular';
import { forkJoin } from 'rxjs';

import { PreOrderService } from '../services/pre-order.service';
import { CustomerSoService } from '../../customers/services/customer-so.service';
import { InventoryService } from '../../inventory/services/inventory.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';
import {
  CreatePreOrderRequest,
  PaymentType,
  PreOrderLineInput,
  UpdatePreOrderRequest,
  WarehouseStockItem,
} from '../models/pre-order.model';

@Component({
  selector: 'app-pre-order-form',
  imports: [
    DxFormModule, DxButtonModule, DxProgressBarModule,
    DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxNumberBoxModule,
    DxDataGridModule, DecimalPipe,
  ],
  templateUrl: './pre-order-form.html',
  styleUrl: './pre-order-form.scss',
})
export class PreOrderForm implements OnInit {
  private readonly svc = inject(PreOrderService);
  private readonly customersSvc = inject(CustomerSoService);
  private readonly inventorySvc = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);

  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);

  protected formData: any = {
    number: '',
    customerId: null,
    warehouseId: null,
    paymentType: '',
    currency: 'KGS',
    expectedDateUtc: null,
    notes: '',
  };

  protected customers: any[] = [];
  protected warehouses: any[] = [];
  protected paymentTypes: PaymentType[] = [];
  protected inventoryItems: any[] = [];
  protected warehouseStock: WarehouseStockItem[] = [];
  protected lines: PreOrderLineInput[] = [];

  protected readonly currencies = ['KGS', 'USD', 'EUR', 'RUB', 'KZT', 'CNY', 'TRY'];

  protected readonly formItems: any[] = [
    { dataField: 'number', label: { text: 'Номер' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
    {
      dataField: 'customerId',
      label: { text: 'Клиент' },
      isRequired: true,
      editorType: 'dxSelectBox',
      editorOptions: {
        dataSource: this.customers,
        displayExpr: 'name',
        valueExpr: 'id',
        stylingMode: 'outlined',
        searchEnabled: true,
      },
    },
    {
      dataField: 'warehouseId',
      label: { text: 'Склад' },
      isRequired: true,
      editorType: 'dxSelectBox',
      editorOptions: {
        dataSource: this.warehouses,
        displayExpr: 'name',
        valueExpr: 'id',
        stylingMode: 'outlined',
        onValueChanged: (e: any) => this.onWarehouseChanged(e.value),
      },
    },
    {
      dataField: 'paymentType',
      label: { text: 'Тип оплаты' },
      isRequired: true,
      editorType: 'dxSelectBox',
      editorOptions: {
        dataSource: this.paymentTypes,
        displayExpr: 'name',
        valueExpr: 'name',
        stylingMode: 'outlined',
      },
    },
    {
      dataField: 'currency',
      label: { text: 'Валюта' },
      editorType: 'dxSelectBox',
      editorOptions: {
        dataSource: this.currencies,
        stylingMode: 'outlined',
      },
    },
    {
      dataField: 'expectedDateUtc',
      label: { text: 'Ожидаемая дата' },
      editorType: 'dxDateBox',
      editorOptions: { stylingMode: 'outlined' },
    },
    {
      dataField: 'notes',
      label: { text: 'Примечания' },
      editorType: 'dxTextBox',
      editorOptions: { stylingMode: 'outlined', height: 80 },
    },
  ];

  ngOnInit() {
    this.loading.set(true);
    const refData$ = forkJoin({
      customers: this.customersSvc.getCustomers(),
      warehouses: this.inventorySvc.getWarehouses(),
      items: this.inventorySvc.getInventoryItems(),
      paymentTypes: this.svc.getPaymentTypes(),
    });

    refData$.subscribe({
      next: (d) => {
        this.customers = d.customers as any[];
        this.warehouses = d.warehouses as any[];
        this.inventoryItems = d.items as any[];
        this.paymentTypes = d.paymentTypes;
        this.refreshFormItems();

        if (this.isEdit) {
          this.svc.getPreOrderById(this.editId!).subscribe({
            next: (po) => {
              this.formData = {
                number: po.number,
                customerId: po.customerId,
                warehouseId: po.warehouseId,
                paymentType: po.paymentType,
                currency: po.currency,
                expectedDateUtc: po.expectedDateUtc,
                notes: po.notes || '',
              };
              this.lines = po.lines.map((l) => ({
                inventoryItemId: l.inventoryItemId,
                quantity: l.quantity,
                unitPrice: l.unitPrice,
                discountPercent: l.discountPercent,
              }));
              if (po.warehouseId) this.onWarehouseChanged(po.warehouseId);
              this.loading.set(false);
            },
            error: (e) => {
              this.toast.showLoad(e);
              this.loading.set(false);
            },
          });
        } else {
          this.loading.set(false);
        }
      },
      error: (e) => {
        this.toast.showLoad(e);
        this.loading.set(false);
      },
    });
  }

  private refreshFormItems() {
    this.formItems[1].editorOptions.dataSource = this.customers;
    this.formItems[2].editorOptions.dataSource = this.warehouses;
    this.formItems[3].editorOptions.dataSource = this.paymentTypes;
  }

  protected onWarehouseChanged(warehouseId: string | null) {
    if (!warehouseId) {
      this.warehouseStock = [];
      return;
    }
    this.svc.getWarehouseStock(warehouseId).subscribe({
      next: (stock) => { this.warehouseStock = stock; },
      error: () => { this.warehouseStock = []; },
    });
  }

  protected getStockForItem(itemId: string): number {
    const s = this.warehouseStock.find((w) => w.inventoryItemId === itemId);
    return s?.quantityOnHand ?? 0;
  }

  protected getStockDiff(itemId: string, qty: number): number {
    return this.getStockForItem(itemId) - qty;
  }

  protected addLine() {
    this.lines.push({ inventoryItemId: '', quantity: 1, unitPrice: 0, discountPercent: 0 });
  }

  protected removeLine(idx: number) {
    this.lines.splice(idx, 1);
  }

  protected getLineTotal(line: PreOrderLineInput): number {
    return line.quantity * line.unitPrice * (1 - line.discountPercent / 100);
  }

  protected getGrandTotal(): number {
    return this.lines.reduce((sum, l) => sum + this.getLineTotal(l), 0);
  }

  protected getItemName(itemId: string): string {
    const item = this.inventoryItems.find((i) => i.id === itemId);
    return item ? `${item.name} (${item.sku})` : '';
  }

  submit() {
    this.saving.set(true);
    const payload: CreatePreOrderRequest = {
      number: this.formData.number,
      customerId: this.formData.customerId,
      warehouseId: this.formData.warehouseId,
      paymentType: this.formData.paymentType,
      currency: this.formData.currency || 'KGS',
      expectedDateUtc: this.formData.expectedDateUtc || null,
      notes: this.formData.notes || null,
      lines: this.lines,
    };

    const done = () => {
      this.saving.set(false);
      void this.router.navigate(['..'], { relativeTo: this.route });
    };
    const fail = (e: any) => {
      this.toast.showSave(e);
      this.saving.set(false);
    };

    if (this.isEdit) {
      const updateReq: UpdatePreOrderRequest = { ...payload, id: this.editId! };
      this.svc.updatePreOrder(this.editId!, updateReq).subscribe({ next: done, error: fail });
    } else {
      this.svc.createPreOrder(payload).subscribe({ next: done, error: fail });
    }
  }

  cancel() {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }
}
