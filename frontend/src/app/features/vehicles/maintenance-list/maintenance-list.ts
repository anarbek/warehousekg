import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule } from 'devextreme-angular';
import { VehiclesService } from '../services/vehicles.service';
import { MaintenanceRecord, Vehicle, CreateMaintenanceRecordRequest } from '../models/vehicles.model';

@Component({
  selector: 'app-maintenance-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule, DatePipe],
  templateUrl: './maintenance-list.html',
  styleUrl: './maintenance-list.scss'
})
export class MaintenanceList {
  private readonly svc = inject(VehiclesService);
  protected readonly items = signal<MaintenanceRecord[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly popupVisible = signal(false);
  protected readonly vehicles = signal<Vehicle[]>([]);
  protected formData: any = { vehicleId: undefined, maintenanceType: 'GeneralCheck', date: new Date().toISOString().slice(0, 10), mileageKm: 0, cost: 0, serviceProvider: '', notes: '', nextDueMileageKm: null, nextDueDate: null };
  protected readonly saving = signal(false);

  protected readonly maintenanceTypes = [
    { id: 'OilChange', name: 'Замена масла' },
    { id: 'TireChange', name: 'Замена шин' },
    { id: 'BrakeService', name: 'Тормозная система' },
    { id: 'EngineService', name: 'Двигатель' },
    { id: 'TransmissionService', name: 'Трансмиссия' },
    { id: 'GeneralCheck', name: 'Общий осмотр' },
    { id: 'Repair', name: 'Ремонт' },
    { id: 'Other', name: 'Прочее' }
  ];

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getAllMaintenanceRecords().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Ошибка загрузки`); this.loading.set(false); }
    });
  }

  protected openAdd() {
    this.vehicles.set([]);
    this.svc.getVehicles().subscribe(v => this.vehicles.set(v));
    this.formData.vehicleId = undefined;
    this.formData.maintenanceType = 'GeneralCheck';
    this.formData.date = new Date().toISOString().slice(0, 10);
    this.formData.mileageKm = 0;
    this.formData.cost = 0;
    this.formData.serviceProvider = '';
    this.formData.notes = '';
    this.formData.nextDueMileageKm = undefined;
    this.formData.nextDueDate = undefined;
    this.popupVisible.set(true);
  }

  protected closePopup() { this.popupVisible.set(false); }
  protected save() {
    if (!this.formData.vehicleId) return;
    this.saving.set(true);
    this.svc.createMaintenanceRecord(this.formData.vehicleId, this.formData).subscribe({
      next: () => { this.saving.set(false); this.popupVisible.set(false); this.load(); },
      error: () => this.saving.set(false)
    });
  }

  protected typeLabel(t: string): string {
    const map: Record<string, string> = {
      OilChange: 'Замена масла', TireChange: 'Замена шин', BrakeService: 'Тормозная система',
      EngineService: 'Двигатель', TransmissionService: 'Трансмиссия', GeneralCheck: 'Общий осмотр',
      Repair: 'Ремонт', Other: 'Прочее'
    };
    return map[t] || t;
  }
}
