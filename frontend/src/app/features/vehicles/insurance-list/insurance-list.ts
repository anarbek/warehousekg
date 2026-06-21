import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule } from 'devextreme-angular';
import { VehiclesService } from '../services/vehicles.service';
import { InsuranceRecord, Vehicle, CreateInsuranceRecordRequest } from '../models/vehicles.model';

@Component({
  selector: 'app-insurance-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DatePipe],
  templateUrl: './insurance-list.html',
  styleUrl: './insurance-list.scss'
})
export class InsuranceList {
  private readonly svc = inject(VehiclesService);
  protected readonly items = signal<InsuranceRecord[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly popupVisible = signal(false);
  protected readonly vehicles = signal<Vehicle[]>([]);
  protected editingId: string | null = null;
  protected formData: any = { vehicleId: undefined, policyNumber: '', provider: '', coverageType: '', startDate: new Date().toISOString().slice(0, 10), endDate: '', premiumAmount: 0 };
  protected readonly saving = signal(false);

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getAllInsuranceRecords().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Ошибка загрузки`); this.loading.set(false); }
    });
  }

  protected openAdd() {
    this.editingId = null;
    this.vehicles.set([]);
    this.svc.getVehicles().subscribe(v => this.vehicles.set(v));
    this.formData.vehicleId = undefined;
    this.formData.policyNumber = '';
    this.formData.provider = '';
    this.formData.coverageType = '';
    this.formData.startDate = new Date().toISOString().slice(0, 10);
    this.formData.endDate = '';
    this.formData.premiumAmount = 0;
    this.popupVisible.set(true);
  }

  protected openEdit(item: InsuranceRecord) {
    this.editingId = item.id;
    this.vehicles.set([]);
    this.svc.getVehicles().subscribe(v => this.vehicles.set(v));
    this.formData.vehicleId = item.vehicleId;
    this.formData.policyNumber = item.policyNumber ?? '';
    this.formData.provider = item.provider ?? '';
    this.formData.coverageType = item.coverageType ?? '';
    this.formData.startDate = item.startDate?.slice(0, 10) ?? '';
    this.formData.endDate = item.endDate?.slice(0, 10) ?? '';
    this.formData.premiumAmount = item.premiumAmount;
    this.popupVisible.set(true);
  }

  protected closePopup() { this.popupVisible.set(false); }
  protected save() {
    if (!this.formData.vehicleId) return;
    this.saving.set(true);
    const req = this.formData as CreateInsuranceRecordRequest;
    const done = () => { this.saving.set(false); this.popupVisible.set(false); this.load(); };
    const fail = () => this.saving.set(false);
    if (this.editingId) {
      (this.svc.updateInsuranceRecord(this.formData.vehicleId, this.editingId, req) as any).subscribe({ next: done, error: fail });
    } else {
      (this.svc.createInsuranceRecord(this.formData.vehicleId, req) as any).subscribe({ next: done, error: fail });
    }
  }

  protected deleteRecord(item: InsuranceRecord) {
    if (!confirm($localize`:@@common.confirmDelete:Удалить запись?`)) return;
    this.svc.deleteInsuranceRecord(item.vehicleId, item.id).subscribe({
      next: () => this.load(),
      error: () => {}
    });
  }

  protected isExpired(dateStr: string): boolean { return new Date(dateStr) < new Date(); }
}
