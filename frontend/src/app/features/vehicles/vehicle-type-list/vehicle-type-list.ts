import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { VehiclesService } from '../services/vehicles.service';
import { VehicleType } from '../models/vehicles.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-vehicle-type-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './vehicle-type-list.html',
  styleUrl: './vehicle-type-list.scss'
})
export class VehicleTypeList {
  private readonly svc = inject(VehiclesService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<VehicleType[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canWrite = computed(() => this.perms.canWrite('vehicleTypes'));
  protected readonly canDelete = computed(() => this.perms.canDelete('vehicleTypes'));

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getVehicleTypes().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Ошибка загрузки`); this.loading.set(false); }
    });
  }

  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteVehicleType(id).subscribe({ next: () => this.load() });
  }
}
