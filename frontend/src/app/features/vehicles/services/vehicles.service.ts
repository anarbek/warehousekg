import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  VehicleType, Vehicle, VehicleDetail,
  DriverAssignment, MaintenanceRecord, InsuranceRecord, InspectionRecord,
  CreateVehicleTypeRequest, UpdateVehicleTypeRequest,
  CreateVehicleRequest, UpdateVehicleRequest,
  CreateDriverAssignmentRequest,
  CreateMaintenanceRecordRequest,
  CreateInsuranceRecordRequest,
  CreateInspectionRecordRequest,
} from '../models/vehicles.model';

@Injectable({ providedIn: 'root' })
export class VehiclesService {
  private readonly http = inject(HttpClient);
  private readonly base = AppSettings.apiBaseUrl;

  // ─── Vehicle Types ────────────────────────────────────────────────
  getVehicleTypes(): Observable<VehicleType[]> { return this.http.get<VehicleType[]>(`${this.base}/vehicle-types`); }
  getVehicleTypeById(id: string): Observable<VehicleType> { return this.http.get<VehicleType>(`${this.base}/vehicle-types/${id}`); }
  createVehicleType(r: CreateVehicleTypeRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicle-types`, r); }
  updateVehicleType(id: string, r: UpdateVehicleTypeRequest): Observable<void> { return this.http.put<void>(`${this.base}/vehicle-types/${id}`, r); }
  deleteVehicleType(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicle-types/${id}`); }

  // ─── Vehicles ─────────────────────────────────────────────────────
  getVehicles(): Observable<Vehicle[]> { return this.http.get<Vehicle[]>(`${this.base}/vehicles`); }
  getVehicleById(id: string): Observable<Vehicle> { return this.http.get<Vehicle>(`${this.base}/vehicles/${id}`); }
  getVehicleDetail(id: string): Observable<VehicleDetail> { return this.http.get<VehicleDetail>(`${this.base}/vehicles/${id}/detail`); }
  createVehicle(r: CreateVehicleRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicles`, r); }
  updateVehicle(id: string, r: UpdateVehicleRequest): Observable<void> { return this.http.put<void>(`${this.base}/vehicles/${id}`, r); }
  deleteVehicle(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicles/${id}`); }

  // ─── Driver Assignments ───────────────────────────────────────────
  getAssignmentsByVehicle(vehicleId: string): Observable<DriverAssignment[]> { return this.http.get<DriverAssignment[]>(`${this.base}/vehicles/${vehicleId}/assignments`); }
  createAssignment(vehicleId: string, r: CreateDriverAssignmentRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicles/${vehicleId}/assignments`, r); }
  updateAssignment(vehicleId: string, id: string, r: { assignedFromUtc: string; assignedToUtc?: string; isPrimary: boolean }): Observable<void> { return this.http.put<void>(`${this.base}/vehicles/${vehicleId}/assignments/${id}`, r); }
  deleteAssignment(vehicleId: string, id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicles/${vehicleId}/assignments/${id}`); }

  // ─── Maintenance ──────────────────────────────────────────────────
  getMaintenanceRecords(vehicleId: string): Observable<MaintenanceRecord[]> { return this.http.get<MaintenanceRecord[]>(`${this.base}/vehicles/${vehicleId}/maintenance`); }
  getAllMaintenanceRecords(): Observable<MaintenanceRecord[]> { return this.http.get<MaintenanceRecord[]>(`${this.base}/maintenance`); }
  createMaintenanceRecord(vehicleId: string, r: CreateMaintenanceRecordRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicles/${vehicleId}/maintenance`, r); }
  updateMaintenanceRecord(vehicleId: string, id: string, r: CreateMaintenanceRecordRequest): Observable<void> { return this.http.put<void>(`${this.base}/vehicles/${vehicleId}/maintenance/${id}`, r); }
  deleteMaintenanceRecord(vehicleId: string, id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicles/${vehicleId}/maintenance/${id}`); }

  // ─── Insurance ────────────────────────────────────────────────────
  getInsuranceRecords(vehicleId: string): Observable<InsuranceRecord[]> { return this.http.get<InsuranceRecord[]>(`${this.base}/vehicles/${vehicleId}/insurance`); }
  getAllInsuranceRecords(): Observable<InsuranceRecord[]> { return this.http.get<InsuranceRecord[]>(`${this.base}/insurance`); }
  createInsuranceRecord(vehicleId: string, r: CreateInsuranceRecordRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicles/${vehicleId}/insurance`, r); }
  updateInsuranceRecord(vehicleId: string, id: string, r: CreateInsuranceRecordRequest): Observable<void> { return this.http.put<void>(`${this.base}/vehicles/${vehicleId}/insurance/${id}`, r); }
  deleteInsuranceRecord(vehicleId: string, id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicles/${vehicleId}/insurance/${id}`); }

  // ─── Inspections ──────────────────────────────────────────────────
  getInspectionRecords(vehicleId: string): Observable<InspectionRecord[]> { return this.http.get<InspectionRecord[]>(`${this.base}/vehicles/${vehicleId}/inspections`); }
  getAllInspectionRecords(): Observable<InspectionRecord[]> { return this.http.get<InspectionRecord[]>(`${this.base}/inspections`); }
  createInspectionRecord(vehicleId: string, r: CreateInspectionRecordRequest): Observable<string> { return this.http.post<string>(`${this.base}/vehicles/${vehicleId}/inspections`, r); }
  updateInspectionRecord(vehicleId: string, id: string, r: CreateInspectionRecordRequest): Observable<void> { return this.http.put<void>(`${this.base}/vehicles/${vehicleId}/inspections/${id}`, r); }
  deleteInspectionRecord(vehicleId: string, id: string): Observable<void> { return this.http.delete<void>(`${this.base}/vehicles/${vehicleId}/inspections/${id}`); }
}
