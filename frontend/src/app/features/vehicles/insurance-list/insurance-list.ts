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

  protected closePopup() { this.popupVisible.set(false); }
  protected save() {
    if (!this.formData.vehicleId) return;
    this.saving.set(true);
    this.svc.createInsuranceRecord(this.formData.vehicleId, this.formData).subscribe({
      next: () => { this.saving.set(false); this.popupVisible.set(false); this.load(); },
      error: () => this.saving.set(false)
    });
  }

  protected isExpired(dateStr: string): boolean { return new Date(dateStr) < new Date(); }
}
