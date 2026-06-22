import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { forkJoin, Observable } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Invoice, InvoiceLine, InvoiceStatus } from '../models/invoice.model';
import { InvoiceService } from '../services/invoice.service';
import { InventoryService } from '../../inventory/services/inventory.service';
import { InventoryItem } from '../../inventory/models/inventory-item.model';

@Component({
  selector: 'app-invoice-detail',
  imports: [DatePipe, DecimalPipe, DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './invoice-detail.html',
  styleUrl: './invoice-detail.scss',
})
export class InvoiceDetail implements OnInit {
  private readonly svc = inject(InvoiceService);
  private readonly inv = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly invoice = signal<Invoice | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly saving = signal(false);

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      inv: this.svc.getInvoiceById(this.id),
      items: this.inv.getInventoryItems(),
    }).subscribe({
      next: ({ inv, items }) => {
        this.invoice.set(inv);
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  canTransition(t: InvoiceStatus): boolean {
    const s = this.invoice()?.status;
    if (!s) return false;
    if (t === 'Issued') return s === 'Draft';
    if (t === 'Printed') return s === 'Issued' || s === 'Printed';
    if (t === 'Signed') return s === 'Issued' || s === 'Printed';
    if (t === 'Cancelled') return s === 'Issued' || s === 'Printed';
    return false;
  }

  transition(a: 'issue' | 'print' | 'sign' | 'cancel') {
    this.saving.set(true);
    let op: Observable<void>;
    switch (a) {
      case 'issue': op = this.svc.issueInvoice(this.id); break;
      case 'print': op = this.svc.printInvoice(this.id); break;
      case 'sign': op = this.svc.signInvoice(this.id); break;
      case 'cancel': op = this.svc.cancelInvoice(this.id); break;
    }
    op.subscribe({
      next: () => { this.saving.set(false); this.load(); },
      error: () => { this.saving.set(false); this.load(); },
    });
  }

  printPdf() {
    const inv = this.invoice();
    if (!inv) return;
    // Mark as printed if in Issued/Printed state, then open PDF via authenticated fetch
    if (inv.status === 'Issued' || inv.status === 'Printed') {
      this.svc.printInvoice(this.id).subscribe({
        next: () => {
          this.invoice.update((i) => i ? { ...i, status: 'Printed' as InvoiceStatus } : null);
          this.openPdfInNewTab();
        },
      });
    } else {
      this.openPdfInNewTab();
    }
  }

  private openPdfInNewTab() {
    this.svc.downloadInvoicePdf(this.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        // Revoke after a short delay to allow the browser to load
        setTimeout(() => URL.revokeObjectURL(url), 5000);
      },
    });
  }

  goBack() {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }

  getItemName(id: string): string {
    const it = this.items().find((i) => i.id === id);
    return it ? `${it.sku} — ${it.name}` : id;
  }

  getLines() {
    const inv = this.invoice();
    if (!inv) return [];
    return inv.lines.map((l: InvoiceLine) => ({
      ...l,
      inventoryItemName: this.getItemName(l.inventoryItemId),
      rowTotal: l.lineTotal + l.taxAmount,
    }));
  }

  sumQtyText = (e: any) => `${e.value}`;
  sumMoneyText = (e: any) => `${(e.value ?? 0).toFixed(2)}`;

  statusText(status: string): string {
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
