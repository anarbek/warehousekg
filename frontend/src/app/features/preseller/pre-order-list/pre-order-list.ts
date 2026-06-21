import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { PreOrderService } from '../services/pre-order.service';
import { PreOrderSummary, PreOrderStatus } from '../models/pre-order.model';
import { ErrorToastService } from '../../../core/services/error-toast.service';

@Component({
  selector: 'app-pre-order-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './pre-order-list.html',
  styleUrl: './pre-order-list.scss',
})
export class PreOrderList {
  private readonly svc = inject(PreOrderService);
  private readonly toast = inject(ErrorToastService);
  protected readonly items = signal<PreOrderSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  private load() {
    this.loading.set(true);
    this.error.set(null);
    this.svc.getPreOrders().subscribe({
      next: (d) => {
        this.items.set(d);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Не удалось загрузить предзаказы');
        this.loading.set(false);
      },
    });
  }

  protected statusText(s: PreOrderStatus): string {
    switch (s) {
      case PreOrderStatus.Draft: return 'Черновик';
      case PreOrderStatus.Submitted: return 'Отправлен';
      case PreOrderStatus.Approved: return 'Одобрен';
      case PreOrderStatus.Rejected: return 'Отклонён';
      case PreOrderStatus.Converted: return 'Конвертирован';
      default: return 'Неизвестно';
    }
  }

  protected statusCss(s: PreOrderStatus): string {
    switch (s) {
      case PreOrderStatus.Draft: return 'status--draft';
      case PreOrderStatus.Submitted: return 'status--submitted';
      case PreOrderStatus.Approved: return 'status--approved';
      case PreOrderStatus.Rejected: return 'status--rejected';
      case PreOrderStatus.Converted: return 'status--converted';
      default: return '';
    }
  }

  deletePreOrder(id: string) {
    if (!confirm('Удалить предзаказ?')) return;
    this.svc.deletePreOrder(id).subscribe({
      next: () => this.load(),
      error: (e) => this.toast.showSave(e),
    });
  }
}
