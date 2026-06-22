import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxSelectBoxModule, DxDateBoxModule } from 'devextreme-angular';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { InvoiceSummary } from '../models/invoice.model';
import { InvoiceService } from '../services/invoice.service';
import { PermissionsService } from '../../../core/services/permissions.service';

const STATUS_OPTIONS = [
  { text: 'Все', value: null },
  { text: 'Черновик', value: 'Draft' },
  { text: 'Выписан', value: 'Issued' },
  { text: 'Напечатан', value: 'Printed' },
  { text: 'Подписан', value: 'Signed' },
  { text: 'Отменён', value: 'Cancelled' },
];

@Component({
  selector: 'app-invoice-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxSelectBoxModule, DxDateBoxModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.scss',
})
export class InvoiceList {
  private readonly svc = inject(InvoiceService);
  private readonly route = inject(ActivatedRoute);
  readonly perms = inject(PermissionsService);

  protected readonly items = signal<InvoiceSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  readonly canDelete = this.perms.canDelete('invoices');
  readonly statusOptions = STATUS_OPTIONS;

  protected filterStatus: any = null;
  protected filterDateFrom: Date | null = null;
  protected filterDateTo: Date | null = null;
  private salesOrderId: string | null = null;

  constructor() {
    this.salesOrderId = this.route.snapshot.queryParamMap.get('salesOrderId');
    this.load();
  }

  private load() {
    this.loading.set(true);
    this.error.set(null);
    const params: any = {};
    if (this.filterStatus) params.status = this.filterStatus;
    if (this.filterDateFrom) params.dateFrom = this.filterDateFrom.toISOString();
    if (this.filterDateTo) params.dateTo = this.filterDateTo.toISOString();
    if (this.salesOrderId) params.salesOrderId = this.salesOrderId;
    this.svc.getInvoices(params).subscribe({
      next: (d) => {
        this.items.set(d);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected onFilterChange() {
    this.load();
  }

  protected async deleteInvoice(id: string) {
    if (!confirm($localize`:@@invoices.confirmDelete:Удалить счёт?`)) return;
    try {
      await firstValueFrom(this.svc.deleteInvoice(id));
      this.load();
    } catch {
      // ignore
    }
  }

  protected statusText(status: string): string {
    const map: Record<string, string> = {
      Draft: $localize`:@@invoice.status.draft:Черновик`,
      Issued: $localize`:@@invoice.status.issued:Выписан`,
      Printed: $localize`:@@invoice.status.printed:Напечатан`,
      Signed: $localize`:@@invoice.status.signed:Подписан`,
      Cancelled: $localize`:@@invoice.status.cancelled:Отменён`,
    };
    return map[status] ?? status;
  }
}
