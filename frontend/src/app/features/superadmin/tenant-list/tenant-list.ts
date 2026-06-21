import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { TenantService } from '../tenant.service';
import { Tenant, TenantStatus } from '../tenant.model';

@Component({
  selector: 'app-tenant-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './tenant-list.html',
  styleUrl: './tenant-list.scss',
})
export class TenantList {
  private readonly service = inject(TenantService);
  protected readonly tenants = signal<Tenant[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly TenantStatus = TenantStatus;

  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getTenants().subscribe({
      next: (data) => {
        this.tenants.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@tenant.list.loadError:Ошибка загрузки арендаторов`);
        this.loading.set(false);
      },
    });
  }

  protected statusText(status: TenantStatus): string {
    switch (status) {
      case TenantStatus.Active: return $localize`:@@tenant.status.active:Активен`;
      case TenantStatus.Suspended: return $localize`:@@tenant.status.suspended:Приостановлен`;
      case TenantStatus.Trial: return $localize`:@@tenant.status.trial:Пробный`;
      default: return '';
    }
  }
}
