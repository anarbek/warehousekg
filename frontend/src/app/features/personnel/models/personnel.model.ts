export interface Position {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface Department {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface Employee {
  id: string;
  code: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  email?: string;
  phone?: string;
  hireDate?: string;
  terminationDate?: string;
  positionId?: string;
  positionName?: string;
  departmentId?: string;
  departmentName?: string;
  applicationUserId?: string;
  linkedUserName?: string;
  isActive: boolean;
}

export interface EmployeeDetailModel extends Employee {
  shiftAssignments: ShiftAssignment[];
  warehouseAssignments: WarehouseAssignment[];
}

export interface ShiftAssignment {
  id: string;
  shiftId: string;
  shiftName: string;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
}

export interface WarehouseAssignment {
  id: string;
  warehouseId: string;
  warehouseName: string;
  isPrimary: boolean;
}

export interface Shift {
  id: string;
  code: string;
  name: string;
  startTime: string;
  endTime: string;
  isActive: boolean;
}

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName?: string;
  shiftId: string;
  shiftName?: string;
  date: string;
  clockInUtc?: string;
  clockOutUtc?: string;
  status: string;
  notes?: string;
}

export interface CreatePositionRequest { code: string; name: string; description?: string; isActive?: boolean; }
export interface UpdatePositionRequest { code: string; name: string; description?: string; isActive: boolean; }
export interface CreateDepartmentRequest { code: string; name: string; description?: string; isActive?: boolean; }
export interface UpdateDepartmentRequest { code: string; name: string; description?: string; isActive: boolean; }
export interface CreateEmployeeRequest {
  code: string; firstName: string; lastName: string; middleName?: string;
  email?: string; phone?: string; hireDate?: string; terminationDate?: string;
  positionId?: string; departmentId?: string; applicationUserId?: string; isActive?: boolean;
}
export interface UpdateEmployeeRequest {
  code: string; firstName: string; lastName: string; middleName?: string;
  email?: string; phone?: string; hireDate?: string; terminationDate?: string;
  positionId?: string; departmentId?: string; applicationUserId?: string; isActive: boolean;
}
export interface CreateShiftRequest { code: string; name: string; startTime: string; endTime: string; isActive?: boolean; }
export interface UpdateShiftRequest { code: string; name: string; startTime: string; endTime: string; isActive: boolean; }
export interface CreateAttendanceRequest {
  employeeId: string; shiftId: string; date: string;
  clockInUtc?: string; clockOutUtc?: string; status: string; notes?: string;
}
export interface UpdateAttendanceRequest {
  employeeId: string; shiftId: string; date: string;
  clockInUtc?: string; clockOutUtc?: string; status: string; notes?: string;
}
