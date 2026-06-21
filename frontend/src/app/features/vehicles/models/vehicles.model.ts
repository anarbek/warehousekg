export interface VehicleType {
  id: string;
  code: string;
  name: string;
  description?: string;
  defaultCapacityKg?: number;
  defaultCapacityM3?: number;
  isActive: boolean;
}

export interface Vehicle {
  id: string;
  code: string;
  licensePlate: string;
  vin?: string;
  brand: string;
  model?: string;
  manufactureYear?: number;
  vehicleTypeId?: string;
  vehicleTypeName?: string;
  ownershipType: string;
  status: string;
  fuelType: string;
  fuelConsumptionRate?: number;
  maxCapacityKg?: number;
  maxCapacityM3?: number;
  currentMileageKm: number;
  purchaseDate?: string;
  purchasePrice?: number;
  insurancePolicyNumber?: string;
  insuranceProvider?: string;
  insuranceExpiryDate?: string;
  techInspectionExpiryDate?: string;
  nextMaintenanceMileageKm?: number;
  nextMaintenanceDate?: string;
  hasGpsTracker: boolean;
  notes?: string;
  assignedDriverName?: string;
}

export interface VehicleDetail extends Vehicle {
  driverAssignments: DriverAssignment[];
  maintenanceRecords: MaintenanceRecord[];
  insuranceRecords: InsuranceRecord[];
  inspectionRecords: InspectionRecord[];
}

export interface DriverAssignment {
  id: string;
  vehicleId: string;
  employeeId: string;
  employeeName?: string;
  assignedFromUtc: string;
  assignedToUtc?: string;
  isPrimary: boolean;
}

export interface MaintenanceRecord {
  id: string;
  vehicleId: string;
  maintenanceType: string;
  date: string;
  mileageKm: number;
  cost: number;
  description?: string;
  serviceProvider?: string;
  notes?: string;
  nextDueMileageKm?: number;
  nextDueDate?: string;
  vehicleCode?: string;
  vehiclePlate?: string;
}

export interface InsuranceRecord {
  id: string;
  vehicleId: string;
  policyNumber: string;
  provider: string;
  coverageType?: string;
  startDate: string;
  endDate: string;
  premiumAmount: number;
  description?: string;
  vehicleCode?: string;
  vehiclePlate?: string;
}

export interface InspectionRecord {
  id: string;
  vehicleId: string;
  inspectionDate: string;
  expiryDate: string;
  result: string;
  inspector?: string;
  notes?: string;
  vehicleCode?: string;
  vehiclePlate?: string;
}

// ─── Request types ──────────────────────────────────────────────────

export interface CreateVehicleTypeRequest {
  code: string;
  name: string;
  description?: string;
  defaultCapacityKg?: number;
  defaultCapacityM3?: number;
  isActive?: boolean;
}

export interface UpdateVehicleTypeRequest {
  code: string;
  name: string;
  description?: string;
  defaultCapacityKg?: number;
  defaultCapacityM3?: number;
  isActive: boolean;
}

export interface CreateVehicleRequest {
  code: string;
  licensePlate: string;
  vin?: string;
  brand: string;
  model?: string;
  manufactureYear?: number;
  vehicleTypeId?: string;
  ownershipType: string;
  status: string;
  fuelType: string;
  fuelConsumptionRate?: number;
  maxCapacityKg?: number;
  maxCapacityM3?: number;
  currentMileageKm: number;
  purchaseDate?: string;
  purchasePrice?: number;
  insurancePolicyNumber?: string;
  insuranceProvider?: string;
  insuranceExpiryDate?: string;
  techInspectionExpiryDate?: string;
  nextMaintenanceMileageKm?: number;
  nextMaintenanceDate?: string;
  hasGpsTracker: boolean;
  notes?: string;
}

export interface UpdateVehicleRequest {
  code: string;
  licensePlate: string;
  vin?: string;
  brand: string;
  model?: string;
  manufactureYear?: number;
  vehicleTypeId?: string;
  ownershipType: string;
  status: string;
  fuelType: string;
  fuelConsumptionRate?: number;
  maxCapacityKg?: number;
  maxCapacityM3?: number;
  currentMileageKm: number;
  purchaseDate?: string;
  purchasePrice?: number;
  insurancePolicyNumber?: string;
  insuranceProvider?: string;
  insuranceExpiryDate?: string;
  techInspectionExpiryDate?: string;
  nextMaintenanceMileageKm?: number;
  nextMaintenanceDate?: string;
  hasGpsTracker: boolean;
  notes?: string;
}

export interface CreateDriverAssignmentRequest {
  employeeId: string;
  assignedFromUtc: string;
  assignedToUtc?: string;
  isPrimary: boolean;
}

export interface CreateMaintenanceRecordRequest {
  maintenanceType: string;
  date: string;
  mileageKm: number;
  cost: number;
  description?: string;
  serviceProvider?: string;
  notes?: string;
  nextDueMileageKm?: number;
  nextDueDate?: string;
}

export interface CreateInsuranceRecordRequest {
  policyNumber: string;
  provider: string;
  coverageType?: string;
  startDate: string;
  endDate: string;
  premiumAmount: number;
  description?: string;
}

export interface CreateInspectionRecordRequest {
  inspectionDate: string;
  expiryDate: string;
  result: string;
  inspector?: string;
  notes?: string;
}
