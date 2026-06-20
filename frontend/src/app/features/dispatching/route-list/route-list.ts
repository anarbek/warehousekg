import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { DispatchingService } from '../services/dispatching.service';
import { DeliveryRoute } from '../models/dispatching.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-route-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './route-list.html',
  styleUrl: './route-list.scss'
})
export class RouteList {
  private readonly svc = inject(DispatchingService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<DeliveryRoute[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canWrite = computed(() => this.perms.canWrite('delivery-routes'));
  protected readonly canDelete = computed(() => this.perms.canDelete('delivery-routes'));

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getRoutes().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => {
        this.error.set($localize`:@@common.loadError:Ошибка загрузки`);
        this.loading.set(false);
      }
    });
  }

  protected statusClass(status: string): string {
    switch (status) {
      case 'Planned': return 'status--planned';
      case 'InProgress': return 'status--active';
      case 'Completed': return 'status--completed';
      default: return 'status--cancelled';
    }
  }

  protected statusLabel(status: string): string {
    switch (status) {
      case 'Planned': return $localize`:@@route.statusPlanned:Запланирован`;
      case 'InProgress': return $localize`:@@route.statusInProgress:В пути`;
      case 'Completed': return $localize`:@@route.statusCompleted:Завершён`;
      default: return $localize`:@@route.statusCancelled:Отменён`;
    }
  }

  protected startRoute(id: string) {
    this.svc.startRoute(id).subscribe({ next: () => this.load() });
  }
  protected completeRoute(id: string) {
    if (!confirm($localize`:@@route.confirmComplete:Завершить маршрут? Оставшиеся остановки будут пропущены.`)) return;
    this.svc.completeRoute(id).subscribe({ next: () => this.load() });
  }
  protected cancelRoute(id: string) {
    if (!confirm($localize`:@@route.confirmCancel:Отменить маршрут?`)) return;
    this.svc.cancelRoute(id).subscribe({ next: () => this.load() });
  }
  protected deleteRoute(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteRoute(id).subscribe({ next: () => this.load() });
  }

  protected canStart(status: string) { return status === 'Planned'; }
  protected canComplete(status: string) { return status === 'InProgress'; }
  protected canCancel(status: string) { return status === 'Planned' || status === 'InProgress'; }
}
