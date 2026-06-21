import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxToolbarModule } from 'devextreme-angular';

import { PreOrderService } from '../services/pre-order.service';
import { InventoryService } from '../../inventory/services/inventory.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';
import { PreOrder, PreOrderStatus } from '../models/pre-order.model';

@Component({
  selector: 'app-pre-order-detail',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxToolbarModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './pre-order-detail.html',
  styleUrl: './pre-order-detail.scss',
})
export class PreOrderDetail implements OnInit {
  private readonly svc = inject(PreOrderService);
  private readonly inv = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly po = signal<PreOrder | null>(null);
  protected readonly loading = signal(false);
  protected readonly working = signal(false);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getPreOrderById(this.id).subscribe({
      next: (d) => {
        this.po.set(d);
        this.loading.set(false);
      },
      error: (e) => {
        this.toast.showLoad(e);
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
      default: return '';
    }
  }

  protected canTransition(target: PreOrderStatus): boolean {
    const s = this.po()?.status;
    switch (target) {
      case PreOrderStatus.Submitted: return s === PreOrderStatus.Draft;
      case PreOrderStatus.Approved: return s === PreOrderStatus.Submitted;
      case PreOrderStatus.Rejected: return s === PreOrderStatus.Submitted;
      case PreOrderStatus.Converted: return s === PreOrderStatus.Approved;
      default: return false;
    }
  }

  transition(action: 'submit' | 'approve' | 'reject' | 'convert') {
    this.working.set(true);
    const done = () => { this.working.set(false); this.load(); };
    const fail = (e: any) => { this.toast.showSave(e); this.working.set(false); };

    switch (action) {
      case 'submit':
        this.svc.submitPreOrder(this.id).subscribe({ next: done, error: fail });
        break;
      case 'approve':
        this.svc.approvePreOrder(this.id).subscribe({ next: done, error: fail });
        break;
      case 'reject':
        this.svc.rejectPreOrder(this.id).subscribe({ next: done, error: fail });
        break;
      case 'convert':
        this.svc.convertPreOrder(this.id).subscribe({
          next: (soId) => {
            this.working.set(false);
            this.load();
          },
          error: fail,
        });
        break;
    }
  }

  deletePreOrder() {
    if (!confirm('Удалить предзаказ?')) return;
    this.working.set(true);
    this.svc.deletePreOrder(this.id).subscribe({
      next: () => {
        this.working.set(false);
        void this.router.navigate(['..'], { relativeTo: this.route });
      },
      error: (e) => {
        this.toast.showSave(e);
        this.working.set(false);
      },
    });
  }
}
