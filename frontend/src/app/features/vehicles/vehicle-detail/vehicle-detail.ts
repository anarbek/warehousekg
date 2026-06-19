import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule } from 'devextreme-angular';
import { VehiclesService } from '../services/vehicles.service';
import { PersonnelService } from '../../personnel/services/personnel.service';
import { VehicleDetail as VehicleDetailModel } from '../models/vehicles.model';
import { Employee } from '../../personnel/models/personnel.model';

@Component({
  selector: 'app-vehicle-detail',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './vehicle-detail.html',
  styleUrl: './vehicle-detail.scss'
})
export class VehicleDetail implements OnInit {
  private readonly svc = inject(VehiclesService);
  private readonly personnel = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private vehicleId = '';
  protected readonly vehicle = signal<VehicleDetailModel | null>(null);
  protected readonly loading = signal(false);
  protected readonly activeTab = signal<'info' | 'maintenance' | 'insurance' | 'inspections'>('info');
  protected readonly saving = signal(false);

  // Popup visibility signals
  protected readonly maintPopup = signal(false);
  protected readonly insurPopup = signal(false);
  protected readonly inspPopup = signal(false);

  // Editing state
  protected editingMaintId = signal<string | null>(null);
  protected editingInsurId = signal<string | null>(null);
  protected editingInspId = signal<string | null>(null);

  // Driver popup
  protected readonly driverPopup = signal(false);
  protected readonly driverEmployees = signal<Employee[]>([]);
  protected driverForm: any = { employeeId: null, assignedFromUtc: new Date().toISOString().slice(0, 10), assignedToUtc: null };

  // Form data
  protected maintForm: any = { maintenanceType: 'GeneralCheck', date: new Date().toISOString().slice(0, 10), mileageKm: 0, cost: 0, serviceProvider: '', notes: '', nextDueMileageKm: null, nextDueDate: null };
  protected insurForm: any = { policyNumber: '', provider: '', coverageType: '', startDate: new Date().toISOString().slice(0, 10), endDate: '', premiumAmount: 0 };
  protected inspForm: any = { inspectionDate: new Date().toISOString().slice(0, 10), expiryDate: '', result: 'Passed', inspector: '', notes: '' };

  protected readonly maintenanceTypes = [
    { id: 'OilChange', name: 'Замена масла' }, { id: 'TireChange', name: 'Замена шин' }, { id: 'BrakeService', name: 'Тормозная система' },
    { id: 'EngineService', name: 'Двигатель' }, { id: 'TransmissionService', name: 'Трансмиссия' }, { id: 'GeneralCheck', name: 'Общий осмотр' },
    { id: 'Repair', name: 'Ремонт' }, { id: 'Other', name: 'Прочее' }
  ];
  protected readonly resultOptions = [
    { id: 'Passed', name: 'Пройден' }, { id: 'Failed', name: 'Не пройден' }, { id: 'RequiresRecheck', name: 'Требует перепроверки' }
  ];

  ngOnInit() {
    this.vehicleId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.vehicleId) return;
    this.loadVehicle();
  }

  private loadVehicle() {
    this.loading.set(true);
    this.svc.getVehicleDetail(this.vehicleId).subscribe({
      next: d => { this.vehicle.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  protected openMaint() { this.editingMaintId.set(null); this.resetMaintForm(); this.maintPopup.set(true); }
  protected openInsur() { this.editingInsurId.set(null); this.resetInsurForm(); this.insurPopup.set(true); }
  protected openInsp() { this.editingInspId.set(null); this.resetInspForm(); this.inspPopup.set(true); }
  protected closeMaint() { this.maintPopup.set(false); this.editingMaintId.set(null); }
  protected closeInsur() { this.insurPopup.set(false); this.editingInsurId.set(null); }
  protected closeInsp() { this.inspPopup.set(false); this.editingInspId.set(null); }

  private resetMaintForm() { Object.assign(this.maintForm, { maintenanceType: 'GeneralCheck', date: new Date().toISOString().slice(0, 10), mileageKm: 0, cost: 0, serviceProvider: '', notes: '', nextDueMileageKm: null, nextDueDate: null }); }
  private resetInsurForm() { Object.assign(this.insurForm, { policyNumber: '', provider: '', coverageType: '', startDate: new Date().toISOString().slice(0, 10), endDate: '', premiumAmount: 0 }); }
  private resetInspForm() { Object.assign(this.inspForm, { inspectionDate: new Date().toISOString().slice(0, 10), expiryDate: '', result: 'Passed', inspector: '', notes: '' }); }

  protected saveMaint() {
    this.saving.set(true);
    const id = this.editingMaintId();
    const req: any = id
      ? this.svc.updateMaintenanceRecord(this.vehicleId, id, this.maintForm)
      : this.svc.createMaintenanceRecord(this.vehicleId, this.maintForm);
    req.subscribe({
      next: () => { this.saving.set(false); this.maintPopup.set(false); this.editingMaintId.set(null); this.loadVehicle(); },
      error: () => this.saving.set(false)
    });
  }
  protected saveInsur() {
    this.saving.set(true);
    const id = this.editingInsurId();
    const req: any = id
      ? this.svc.updateInsuranceRecord(this.vehicleId, id, this.insurForm)
      : this.svc.createInsuranceRecord(this.vehicleId, this.insurForm);
    req.subscribe({
      next: () => { this.saving.set(false); this.insurPopup.set(false); this.editingInsurId.set(null); this.loadVehicle(); },
      error: () => this.saving.set(false)
    });
  }
  protected saveInsp() {
    this.saving.set(true);
    const id = this.editingInspId();
    const req: any = id
      ? this.svc.updateInspectionRecord(this.vehicleId, id, this.inspForm)
      : this.svc.createInspectionRecord(this.vehicleId, this.inspForm);
    req.subscribe({
      next: () => { this.saving.set(false); this.inspPopup.set(false); this.editingInspId.set(null); this.loadVehicle(); },
      error: () => this.saving.set(false)
    });
  }

  protected editMaint(record: any) {
    this.editingMaintId.set(record.id);
    Object.assign(this.maintForm, {
      maintenanceType: record.maintenanceType, date: this.isoToDateStr(record.date),
      mileageKm: record.mileageKm, cost: record.cost,
      serviceProvider: record.serviceProvider || '', notes: record.notes || '',
      nextDueMileageKm: record.nextDueMileageKm, nextDueDate: this.isoToDateStr(record.nextDueDate)
    });
    this.maintPopup.set(true);
  }
  protected editInsur(record: any) {
    this.editingInsurId.set(record.id);
    Object.assign(this.insurForm, {
      policyNumber: record.policyNumber, provider: record.provider,
      coverageType: record.coverageType || '',
      startDate: this.isoToDateStr(record.startDate), endDate: this.isoToDateStr(record.endDate),
      premiumAmount: record.premiumAmount
    });
    this.insurPopup.set(true);
  }
  protected editInsp(record: any) {
    this.editingInspId.set(record.id);
    Object.assign(this.inspForm, {
      inspectionDate: this.isoToDateStr(record.inspectionDate),
      expiryDate: this.isoToDateStr(record.expiryDate),
      result: record.result, inspector: record.inspector || '', notes: record.notes || ''
    });
    this.inspPopup.set(true);
  }

  private isoToDateStr(iso: string | undefined | null): string | null {
    if (!iso) return null;
    const m = iso.match(/^(\d{4})-(\d{2})-(\d{2})/);
    return m ? `${m[1]}-${m[2]}-${m[3]}` : null;
  }

  protected deleteMaint(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteMaintenanceRecord(this.vehicleId, id).subscribe({ next: () => this.loadVehicle() });
  }
  protected deleteInsur(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteInsuranceRecord(this.vehicleId, id).subscribe({ next: () => this.loadVehicle() });
  }
  protected deleteInsp(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteInspectionRecord(this.vehicleId, id).subscribe({ next: () => this.loadVehicle() });
  }

  // ─── Driver management ──────────────────────────────────────────
  protected openDriverPopup() {
    this.driverForm.employeeId = null;
    this.driverForm.assignedFromUtc = new Date().toISOString().slice(0, 10);
    this.driverForm.assignedToUtc = null;
    this.personnel.getEmployees().subscribe(e => this.driverEmployees.set(e));
    this.driverPopup.set(true);
  }
  protected closeDriverPopup() { this.driverPopup.set(false); }
  protected saveDriver() {
    if (!this.driverForm.employeeId) return;
    this.svc.createAssignment(this.vehicleId, {
      employeeId: this.driverForm.employeeId,
      assignedFromUtc: this.driverForm.assignedFromUtc,
      assignedToUtc: this.driverForm.assignedToUtc || undefined,
      isPrimary: true
    }).subscribe({ next: () => { this.driverPopup.set(false); this.loadVehicle(); } });
  }
  protected deleteDriver(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteAssignment(this.vehicleId, id).subscribe({ next: () => this.loadVehicle() });
  }

  protected statusLabel(s: string): string {
    const map: Record<string, string> = { Active: 'Активен', InMaintenance: 'На ТО', OutOfService: 'Не работает', Decommissioned: 'Списан' };
    return map[s] || s;
  }

  protected ownershipLabel(o: string): string {
    const map: Record<string, string> = { Owned: 'Собственный', Leased: 'Лизинг', Rented: 'Аренда' };
    return map[o] || o;
  }

  protected maintenanceTypeLabel(t: string): string {
    const map: Record<string, string> = {
      OilChange: 'Замена масла', TireChange: 'Замена шин', BrakeService: 'Тормозная система',
      EngineService: 'Двигатель', TransmissionService: 'Трансмиссия', GeneralCheck: 'Общий осмотр',
      Repair: 'Ремонт', Other: 'Прочее'
    };
    return map[t] || t;
  }
}
