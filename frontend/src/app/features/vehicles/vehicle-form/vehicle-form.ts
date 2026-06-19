import { Component, inject, signal, OnInit } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule, DxSelectBoxModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { VehiclesService } from '../services/vehicles.service';
import { CreateVehicleRequest, UpdateVehicleRequest, VehicleType, Vehicle } from '../models/vehicles.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-vehicle-form',
  imports: [DxFormModule, DxButtonModule, DxProgressBarModule, DxSelectBoxModule, RouterLink],
  templateUrl: './vehicle-form.html',
  styleUrl: './vehicle-form.scss'
})
export class VehicleForm implements OnInit {
  private readonly svc = inject(VehiclesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly loading = signal(false);

  protected readonly vehicleTypes = signal<VehicleType[]>([]);
  protected readonly formItems = signal<any[]>([]);

  protected readonly ownershipTypes = [
    { value: 'Owned', text: 'Собственный' },
    { value: 'Leased', text: 'Лизинг' },
    { value: 'Rented', text: 'Аренда' },
  ];
  protected readonly statuses = [
    { value: 'Active', text: 'Активен' },
    { value: 'InMaintenance', text: 'На ТО' },
    { value: 'OutOfService', text: 'Не работает' },
    { value: 'Decommissioned', text: 'Списан' },
  ];
  protected readonly fuelTypes = [
    { value: 'Diesel', text: 'Дизель' },
    { value: 'Gasoline', text: 'Бензин' },
    { value: 'Electric', text: 'Электро' },
    { value: 'Hybrid', text: 'Гибрид' },
    { value: 'LPG', text: 'Газ (LPG)' },
    { value: 'CNG', text: 'Газ (CNG)' },
  ];

  protected formData: any = {
    code: '', licensePlate: '', vin: '', brand: '', model: '', manufactureYear: null,
    vehicleTypeId: null, ownershipType: 'Owned', status: 'Active', fuelType: 'Diesel',
    fuelConsumptionRate: null, maxCapacityKg: null, maxCapacityM3: null,
    currentMileageKm: 0, purchaseDate: null, purchasePrice: null,
    insurancePolicyNumber: '', insuranceProvider: '', insuranceExpiryDate: null,
    techInspectionExpiryDate: null, nextMaintenanceMileageKm: null, nextMaintenanceDate: null,
    hasGpsTracker: false, notes: ''
  };

  ngOnInit() {
    this.loading.set(true);
    const loads: any[] = [this.svc.getVehicleTypes()];
    if (this.isEdit && this.editId) {
      loads.push(this.svc.getVehicleById(this.editId));
    }
    forkJoin(loads).subscribe({
      next: (results: any[]) => {
        this.vehicleTypes.set(results[0] as VehicleType[]);
        if (this.isEdit && results[1]) {
          Object.assign(this.formData, results[1] as Vehicle);
        }
        this.formItems.set(this.buildFormItems());
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private buildFormItems(): any[] {
    return [
      { dataField: 'code', label: { text: 'Код' }, editorType: 'dxTextBox', isRequired: true, editorOptions: { placeholder: 'Код' } },
      { dataField: 'licensePlate', label: { text: 'Госномер' }, editorType: 'dxTextBox', isRequired: true, editorOptions: { placeholder: 'Госномер' } },
      { dataField: 'brand', label: { text: 'Марка' }, editorType: 'dxTextBox', isRequired: true, editorOptions: { placeholder: 'Марка' } },
      { dataField: 'model', label: { text: 'Модель' }, editorType: 'dxTextBox', editorOptions: { placeholder: 'Модель' } },
      { dataField: 'vin', label: { text: 'VIN' }, editorType: 'dxTextBox', editorOptions: { placeholder: 'VIN' } },
      { dataField: 'manufactureYear', label: { text: 'Год выпуска' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Год выпуска', showSpinButtons: true } },
      { dataField: 'vehicleTypeId', label: { text: 'Тип транспорта' }, editorType: 'dxSelectBox', editorOptions: { dataSource: this.vehicleTypes(), displayExpr: 'name', valueExpr: 'id', placeholder: 'Тип транспорта' } },
      { dataField: 'ownershipType', label: { text: 'Владение' }, editorType: 'dxSelectBox', editorOptions: { dataSource: this.ownershipTypes, displayExpr: 'text', valueExpr: 'value' } },
      { dataField: 'status', label: { text: 'Статус' }, editorType: 'dxSelectBox', editorOptions: { dataSource: this.statuses, displayExpr: 'text', valueExpr: 'value' } },
      { dataField: 'fuelType', label: { text: 'Топливо' }, editorType: 'dxSelectBox', editorOptions: { dataSource: this.fuelTypes, displayExpr: 'text', valueExpr: 'value' } },
      { dataField: 'fuelConsumptionRate', label: { text: 'Расход (л/100км)' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Расход (л/100км)', format: '#0.##' } },
      { dataField: 'maxCapacityKg', label: { text: 'Грузоподъёмность (кг)' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Грузоподъёмность (кг)', format: '#,##0.###' } },
      { dataField: 'maxCapacityM3', label: { text: 'Объём кузова (м³)' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Объём кузова (м³)', format: '#,##0.###' } },
      { dataField: 'currentMileageKm', label: { text: 'Пробег (км)' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Пробег (км)', format: '#,##0.#' } },
      { dataField: 'hasGpsTracker', label: { text: 'GPS трекер' }, editorType: 'dxSwitch', editorOptions: { switchedOnText: 'GPS есть', switchedOffText: 'Нет GPS' } },
      { dataField: 'purchaseDate', label: { text: 'Дата покупки' }, editorType: 'dxDateBox', editorOptions: { placeholder: 'Дата покупки' } },
      { dataField: 'purchasePrice', label: { text: 'Цена покупки' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'Цена покупки', format: '#,##0.00' } },
      { dataField: 'insurancePolicyNumber', label: { text: '№ полиса' }, editorType: 'dxTextBox', editorOptions: { placeholder: '№ полиса' } },
      { dataField: 'insuranceProvider', label: { text: 'Страховая компания' }, editorType: 'dxTextBox', editorOptions: { placeholder: 'Страховая компания' } },
      { dataField: 'insuranceExpiryDate', label: { text: 'Страховка до' }, editorType: 'dxDateBox', editorOptions: { placeholder: 'Страховка до' } },
      { dataField: 'techInspectionExpiryDate', label: { text: 'Техосмотр до' }, editorType: 'dxDateBox', editorOptions: { placeholder: 'Техосмотр до' } },
      { dataField: 'nextMaintenanceMileageKm', label: { text: 'След. ТО (км)' }, editorType: 'dxNumberBox', editorOptions: { placeholder: 'След. ТО (км)', format: '#,##0.#' } },
      { dataField: 'nextMaintenanceDate', label: { text: 'След. ТО (дата)' }, editorType: 'dxDateBox', editorOptions: { placeholder: 'След. ТО (дата)' } },
      { dataField: 'notes', label: { text: 'Заметки' }, editorType: 'dxTextArea', colSpan: 2, editorOptions: { placeholder: 'Заметки', height: 80 } },
    ];
  }

  protected submit() {
    this.loading.set(true);
    if (this.isEdit && this.editId) {
      this.svc.updateVehicle(this.editId, this.formData as UpdateVehicleRequest).subscribe({
        next: () => { this.loading.set(false); this.router.navigate(['/vehicles/list']); },
        error: () => this.loading.set(false)
      });
    } else {
      this.svc.createVehicle(this.formData as CreateVehicleRequest).subscribe({
        next: () => { this.loading.set(false); this.router.navigate(['/vehicles/list']); },
        error: () => this.loading.set(false)
      });
    }
  }
}
