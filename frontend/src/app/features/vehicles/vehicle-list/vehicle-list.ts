import { Component, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { VehiclesService } from '../services/vehicles.service';
import { Vehicle } from '../models/vehicles.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-vehicle-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink, DatePipe],
  templateUrl: './vehicle-list.html',
  styleUrl: './vehicle-list.scss'
})
export class VehicleList {
  private readonly svc = inject(VehiclesService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Vehicle[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canWrite = computed(() => this.perms.canWrite('vehicles'));
  protected readonly canDelete = computed(() => this.perms.canDelete('vehicles'));

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getVehicles().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Ошибка загрузки`); this.loading.set(false); }
    });
  }

  protected statusClass(status: string): string {
    switch (status) {
      case 'Active': return 'status--active';
      case 'InMaintenance': return 'status--maintenance';
      case 'OutOfService': return 'status--out';
      default: return 'status--decommissioned';
    }
  }

  protected statusLabel(status: string): string {
    switch (status) {
      case 'Active': return 'Активен';
      case 'InMaintenance': return 'На ТО';
      case 'OutOfService': return 'Не работает';
      default: return 'Списан';
    }
  }

  protected isExpired(dateStr?: string): boolean {
    if (!dateStr) return false;
    return new Date(dateStr) < new Date();
  }

  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteVehicle(id).subscribe({ next: () => this.load() });
  }
}
