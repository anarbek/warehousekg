import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule } from 'devextreme-angular';
import { VehiclesService } from '../services/vehicles.service';
import { InspectionRecord, Vehicle, CreateInspectionRecordRequest } from '../models/vehicles.model';

@Component({
  selector: 'app-inspection-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxTextAreaModule, DatePipe],
  templateUrl: './inspection-list.html',
  styleUrl: './inspection-list.scss'
})
export class InspectionList {
  private readonly svc = inject(VehiclesService);
  protected readonly items = signal<InspectionRecord[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly popupVisible = signal(false);
  protected readonly vehicles = signal<Vehicle[]>([]);
  protected editingId: string | null = null;
  protected formData: any = { vehicleId: undefined, inspectionDate: new Date().toISOString().slice(0, 10), expiryDate: '', result: 'Passed', inspector: '', notes: '' };
  protected readonly saving = signal(false);

  protected readonly resultOptions = [
    { id: 'Passed', name: 'Пройден' },
    { id: 'Failed', name: 'Не пройден' },
    { id: 'RequiresRecheck', name: 'Требует перепроверки' }
  ];

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getAllInspectionRecords().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Ошибка загрузки`); this.loading.set(false); }
    });
  }

  protected openAdd() {
    this.editingId = null;
    this.vehicles.set([]);
    this.svc.getVehicles().subscribe(v => this.vehicles.set(v));
    this.formData.vehicleId = undefined;
    this.formData.inspectionDate = new Date().toISOString().slice(0, 10);
    this.formData.expiryDate = '';
    this.formData.result = 'Passed';
    this.formData.inspector = '';
    this.formData.notes = '';
    this.popupVisible.set(true);
  }

  protected openEdit(item: InspectionRecord) {
    this.editingId = item.id;
    this.vehicles.set([]);
    this.svc.getVehicles().subscribe(v => this.vehicles.set(v));
    this.formData.vehicleId = item.vehicleId;
    this.formData.inspectionDate = item.inspectionDate?.slice(0, 10) ?? '';
    this.formData.expiryDate = item.expiryDate?.slice(0, 10) ?? '';
    this.formData.result = item.result ?? 'Passed';
    this.formData.inspector = item.inspector ?? '';
    this.formData.notes = item.notes ?? '';
    this.popupVisible.set(true);
  }

  protected closePopup() { this.popupVisible.set(false); }
  protected save() {
    if (!this.formData.vehicleId) return;
    this.saving.set(true);
    const req = this.formData as CreateInspectionRecordRequest;
    const done = () => { this.saving.set(false); this.popupVisible.set(false); this.load(); };
    const fail = () => this.saving.set(false);
    if (this.editingId) {
      (this.svc.updateInspectionRecord(this.formData.vehicleId, this.editingId, req) as any).subscribe({ next: done, error: fail });
    } else {
      (this.svc.createInspectionRecord(this.formData.vehicleId, req) as any).subscribe({ next: done, error: fail });
    }
  }

  protected deleteRecord(item: InspectionRecord) {
    if (!confirm($localize`:@@common.confirmDelete:Удалить запись?`)) return;
    this.svc.deleteInspectionRecord(item.vehicleId, item.id).subscribe({
      next: () => this.load(),
      error: () => {}
    });
  }
}
