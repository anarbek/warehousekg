import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  Position, Department, Employee, EmployeeDetailModel, Shift, AttendanceRecord,
  CreatePositionRequest, UpdatePositionRequest,
  CreateDepartmentRequest, UpdateDepartmentRequest,
  CreateEmployeeRequest, UpdateEmployeeRequest,
  CreateShiftRequest, UpdateShiftRequest,
  CreateAttendanceRequest, UpdateAttendanceRequest
} from '../models/personnel.model';

@Injectable({ providedIn: 'root' })
export class PersonnelService {
  private readonly http = inject(HttpClient);
  private readonly base = AppSettings.apiBaseUrl;

  // ─── Positions ────────────────────────────────────────────────────
  getPositions(): Observable<Position[]> { return this.http.get<Position[]>(`${this.base}/positions`); }
  getPositionById(id: string): Observable<Position> { return this.http.get<Position>(`${this.base}/positions/${id}`); }
  createPosition(r: CreatePositionRequest): Observable<string> { return this.http.post<string>(`${this.base}/positions`, r); }
  updatePosition(id: string, r: UpdatePositionRequest): Observable<void> { return this.http.put<void>(`${this.base}/positions/${id}`, r); }
  deletePosition(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/positions/${id}`); }

  // ─── Departments ──────────────────────────────────────────────────
  getDepartments(): Observable<Department[]> { return this.http.get<Department[]>(`${this.base}/departments`); }
  getDepartmentById(id: string): Observable<Department> { return this.http.get<Department>(`${this.base}/departments/${id}`); }
  createDepartment(r: CreateDepartmentRequest): Observable<string> { return this.http.post<string>(`${this.base}/departments`, r); }
  updateDepartment(id: string, r: UpdateDepartmentRequest): Observable<void> { return this.http.put<void>(`${this.base}/departments/${id}`, r); }
  deleteDepartment(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/departments/${id}`); }

  // ─── Employees ────────────────────────────────────────────────────
  getEmployees(): Observable<Employee[]> { return this.http.get<Employee[]>(`${this.base}/employees`); }
  getEmployeeById(id: string): Observable<Employee> { return this.http.get<Employee>(`${this.base}/employees/${id}`); }
  getEmployeeDetail(id: string): Observable<EmployeeDetailModel> { return this.http.get<EmployeeDetailModel>(`${this.base}/employees/${id}/detail`); }
  createEmployee(r: CreateEmployeeRequest): Observable<string> { return this.http.post<string>(`${this.base}/employees`, r); }
  updateEmployee(id: string, r: UpdateEmployeeRequest): Observable<void> { return this.http.put<void>(`${this.base}/employees/${id}`, r); }
  deleteEmployee(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/employees/${id}`); }

  // ─── Shifts ───────────────────────────────────────────────────────
  getShifts(): Observable<Shift[]> { return this.http.get<Shift[]>(`${this.base}/shifts`); }
  getShiftById(id: string): Observable<Shift> { return this.http.get<Shift>(`${this.base}/shifts/${id}`); }
  createShift(r: CreateShiftRequest): Observable<string> { return this.http.post<string>(`${this.base}/shifts`, r); }
  updateShift(id: string, r: UpdateShiftRequest): Observable<void> { return this.http.put<void>(`${this.base}/shifts/${id}`, r); }
  deleteShift(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/shifts/${id}`); }

  // ─── Attendance ──────────────────────────────────────────────────
  getAttendance(from?: string, to?: string): Observable<AttendanceRecord[]> {
    let url = `${this.base}/attendance`;
    const params: string[] = [];
    if (from) params.push(`from=${from}`);
    if (to) params.push(`to=${to}`);
    if (params.length) url += '?' + params.join('&');
    return this.http.get<AttendanceRecord[]>(url);
  }
  getAttendanceById(id: string): Observable<AttendanceRecord> { return this.http.get<AttendanceRecord>(`${this.base}/attendance/${id}`); }
  createAttendance(r: CreateAttendanceRequest): Observable<string> { return this.http.post<string>(`${this.base}/attendance`, r); }
  updateAttendance(id: string, r: UpdateAttendanceRequest): Observable<void> { return this.http.put<void>(`${this.base}/attendance/${id}`, r); }
  deleteAttendance(id: string): Observable<void> { return this.http.delete<void>(`${this.base}/attendance/${id}`); }
}
