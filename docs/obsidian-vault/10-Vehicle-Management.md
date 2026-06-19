# Vehicle Management Module

Domain entities, API endpoints, frontend routes, and workflows for the fleet/vehicle management module.

## Overview

The Vehicle Management module covers:
- **Vehicles** — transport units with specifications, ownership, status tracking
- **Vehicle Types** — lookup catalog (truck, van, etc.) with default capacity
- **Driver Assignments** — time-range-based driver-to-vehicle assignments
- **Maintenance Records** — service history with cost, provider, next-due tracking
- **Insurance Records** — policy tracking with provider, coverage, premium
- **Inspection Records** — technical inspections with results and expiry

All entities are tenant-scoped via `BaseEntity`.

---

## Database Schema

### `vehicle_types`
Lookup catalog for vehicle types.

| Column | Type | Notes |
|---|---|---|
| `Code` | `string(32)` | Unique per tenant |
| `Name` | `string(128)` | Display name |
| `Description` | `string(512)` | Optional |
| `DefaultCapacityKg` | `decimal(18,1)` | Default cargo capacity |
| `DefaultCapacityM3` | `decimal(18,2)` | Default volume capacity |
| `IsActive` | `bool` | Soft disable flag |

### `vehicles`
Main vehicle aggregate.

| Column | Type | Notes |
|---|---|---|
| `Code` | `string(32)` | Unique per tenant |
| `LicensePlate` | `string(16)` | License plate number |
| `VIN` | `string(32)` | Optional VIN |
| `Brand` | `string(64)` | Manufacturer |
| `Model` | `string(64)` | Optional model name |
| `ManufactureYear` | `int` | Optional |
| `VehicleTypeId` | `Guid?` | FK → `vehicle_types` |
| `OwnershipType` | `enum` | Owned / Leased / Rented |
| `Status` | `enum` | Active / InMaintenance / OutOfService / Decommissioned |
| `FuelType` | `enum` | Diesel / Gasoline / Electric / Hybrid / LPG / CNG |
| `FuelConsumptionRate` | `decimal(6,2)` | L/100km |
| `MaxCapacityKg` | `decimal(18,1)` | Cargo capacity override |
| `MaxCapacityM3` | `decimal(18,2)` | Volume capacity override |
| `CurrentMileageKm` | `decimal(18,1)` | Odometer reading |
| `PurchaseDate` | `DateTime?` | Purchase date |
| `PurchasePrice` | `decimal(18,2)` | Purchase price |
| `InsurancePolicyNumber` | `string(64)` | Cached from latest insurance |
| `InsuranceProvider` | `string(128)` | Cached from latest insurance |
| `InsuranceExpiryDate` | `DateTime?` | Auto-updated on insurance create |
| `TechInspectionExpiryDate` | `DateTime?` | Auto-updated on inspection create |
| `NextMaintenanceMileageKm` | `decimal(18,1)` | Auto-updated on maintenance create |
| `NextMaintenanceDate` | `DateTime?` | Auto-updated on maintenance create |
| `HasGpsTracker` | `bool` | GPS tracker flag |
| `Notes` | `string(1024)` | Optional |

> **DateTime handling**: All DateTime values are stored as UTC (`timestamp with time zone` in PostgreSQL). Application code uses `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before saving. Frontend date boxes use `dateSerializationFormat="yyyy-MM-dd"` and form data uses plain `YYYY-MM-DD` strings (not Date objects) to avoid timezone shifts.

### `vehicle_driver_assignments`
Time-range driver-to-vehicle assignments.

| Column | Type | Notes |
|---|---|---|
| `VehicleId` | `Guid` | FK → `vehicles` (Cascade) |
| `EmployeeId` | `Guid` | FK → `employees` (Restrict) |
| `AssignedFromUtc` | `DateTime` | Assignment start |
| `AssignedToUtc` | `DateTime?` | Assignment end (null = indefinite) |
| `IsPrimary` | `bool` | Primary driver flag |

### `vehicle_maintenance_records`
Service/maintenance history.

| Column | Type | Notes |
|---|---|---|
| `VehicleId` | `Guid` | FK → `vehicles` (Cascade) |
| `MaintenanceType` | `enum` | OilChange / TireChange / BrakeService / EngineService / TransmissionService / GeneralCheck / Repair / Other |
| `Date` | `DateTime` | Service date |
| `MileageKm` | `decimal(18,1)` | Odometer at service |
| `Cost` | `decimal(18,2)` | Service cost |
| `Description` | `string(1024)` | Optional |
| `ServiceProvider` | `string(256)` | Optional |
| `Notes` | `string(1024)` | Optional |
| `NextDueMileageKm` | `decimal(18,1)` | Next service due at |
| `NextDueDate` | `DateTime?` | Next service due date |

### `vehicle_insurance_records`
Insurance policy history.

| Column | Type | Notes |
|---|---|---|
| `VehicleId` | `Guid` | FK → `vehicles` (Cascade) |
| `PolicyNumber` | `string(64)` | Policy number |
| `Provider` | `string(128)` | Insurance provider |
| `CoverageType` | `string(64)` | Coverage type |
| `StartDate` | `DateTime` | Policy start |
| `EndDate` | `DateTime` | Policy expiry |
| `PremiumAmount` | `decimal(18,2)` | Premium cost |
| `Description` | `string(1024)` | Optional |

### `vehicle_inspection_records`
Technical inspection history.

| Column | Type | Notes |
|---|---|---|
| `VehicleId` | `Guid` | FK → `vehicles` (Cascade) |
| `InspectionDate` | `DateTime` | Inspection date |
| `ExpiryDate` | `DateTime` | Valid until |
| `Result` | `enum` | Passed / Failed / RequiresRecheck |
| `Inspector` | `string(128)` | Inspector name |
| `Notes` | `string(1024)` | Optional |

### `vehicle_fuel_records`
Fuel consumption log (entity exists, CRUD not yet implemented).

---

## Enums

| Enum | Values |
|---|---|
| `VehicleStatus` | Active, InMaintenance, OutOfService, Decommissioned |
| `VehicleOwnershipType` | Owned, Leased, Rented |
| `MaintenanceType` | OilChange, TireChange, BrakeService, EngineService, TransmissionService, GeneralCheck, Repair, Other |
| `InspectionResult` | Passed, Failed, RequiresRecheck |
| `FuelType` | Diesel, Gasoline, Electric, Hybrid, LPG, CNG |

All enums use `[JsonConverter(typeof(JsonStringEnumConverter))]` for string-based serialization.

---

## API Endpoints

Base: `/api/v1`

### Vehicle Types
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicle-types` | List all vehicle types |
| `GET` | `/vehicle-types/{id}` | Get by ID |
| `POST` | `/vehicle-types` | Create |
| `PUT` | `/vehicle-types/{id}` | Update |
| `DELETE` | `/vehicle-types/{id}` | Delete |

### Vehicles
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicles` | List all vehicles |
| `GET` | `/vehicles/{id}` | Get by ID |
| `GET` | `/vehicles/{id}/detail` | Get with nested records (maintenance, insurance, inspections, drivers) |
| `POST` | `/vehicles` | Create |
| `PUT` | `/vehicles/{id}` | Update |
| `DELETE` | `/vehicles/{id}` | Delete |

### Driver Assignments
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicles/{vehicleId}/assignments` | Get assignments for vehicle |
| `GET` | `/vehicles/assignments/employee/{employeeId}` | Get assignments for employee |
| `POST` | `/vehicles/{vehicleId}/assignments` | Create assignment |
| `PUT` | `/vehicles/{vehicleId}/assignments/{id}` | Update assignment |
| `DELETE` | `/vehicles/{vehicleId}/assignments/{id}` | Delete assignment |

### Maintenance (per-vehicle)
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicles/{vehicleId}/maintenance` | Get records for vehicle |
| `POST` | `/vehicles/{vehicleId}/maintenance` | Create record |
| `PUT` | `/vehicles/{vehicleId}/maintenance/{id}` | Update record |
| `DELETE` | `/vehicles/{vehicleId}/maintenance/{id}` | Delete record |

### Maintenance (fleet-wide)
| Method | Route | Description |
|---|---|---|
| `GET` | `/maintenance` | Get all maintenance records |

### Insurance (per-vehicle)
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicles/{vehicleId}/insurance` | Get records for vehicle |
| `POST` | `/vehicles/{vehicleId}/insurance` | Create record |
| `PUT` | `/vehicles/{vehicleId}/insurance/{id}` | Update record |
| `DELETE` | `/vehicles/{vehicleId}/insurance/{id}` | Delete record |

### Insurance (fleet-wide)
| Method | Route | Description |
|---|---|---|
| `GET` | `/insurance` | Get all insurance records |

### Inspections (per-vehicle)
| Method | Route | Description |
|---|---|---|
| `GET` | `/vehicles/{vehicleId}/inspections` | Get records for vehicle |
| `POST` | `/vehicles/{vehicleId}/inspections` | Create record |
| `PUT` | `/vehicles/{vehicleId}/inspections/{id}` | Update record |
| `DELETE` | `/vehicles/{vehicleId}/inspections/{id}` | Delete record |

### Inspections (fleet-wide)
| Method | Route | Description |
|---|---|---|
| `GET` | `/inspections` | Get all inspection records |

---

## Frontend Routes

Lazy-loaded under `/vehicles`:

| Route | Component | Description |
|---|---|---|
| `/vehicles/list` | `VehicleList` | Vehicles grid |
| `/vehicles/new` | `VehicleForm` | Create vehicle |
| `/vehicles/:id` | `VehicleDetail` | Vehicle detail with tabs |
| `/vehicles/:id/edit` | `VehicleForm` | Edit vehicle |
| `/vehicles/types` | `VehicleTypeList` | Vehicle types grid |
| `/vehicles/types/new` | `VehicleTypeForm` | Create type |
| `/vehicles/types/:id/edit` | `VehicleTypeForm` | Edit type |
| `/vehicles/maintenance` | `MaintenanceList` | Fleet-wide maintenance |
| `/vehicles/insurance` | `InsuranceList` | Fleet-wide insurance |
| `/vehicles/inspections` | `InspectionList` | Fleet-wide inspections |

### Vehicle Detail Tabs
The vehicle detail page has tabs:
- **Информация** — vehicle info, specs, insurance/TO summary
- **ТО** — maintenance records with Add/Edit/Delete
- **Страховка** — insurance records with Add/Edit/Delete
- **Техосмотр** — inspection records with Add/Edit/Delete
- **Водители** — driver assignments with Add/Delete, date range picker

### Navigation
Sidebar menu under "Автопарк":
- Транспорт → `/vehicles/list`
- Типы транспорта → `/vehicles/types`
- ТО → `/vehicles/maintenance`
- Страховка → `/vehicles/insurance`
- Техосмотр → `/vehicles/inspections`

---

## Architecture Notes

### CQRS Pattern
All operations follow CQRS with MediatR:
- **Commands**: Record types implementing `IRequest<T>`, handlers in `Application/Features/Vehicles/*/Commands/`
- **Queries**: Record types implementing `IRequest<T>`, handlers in `Application/Features/Vehicles/*/Queries/`
- **DTOs**: Manual projections in `Application/Features/Vehicles/Vehicles/Dtos/VehicleDtos.cs`

### Cross-cutting Updates
- Creating a maintenance record auto-updates `Vehicle.NextMaintenanceMileageKm` and `Vehicle.NextMaintenanceDate`
- Creating an insurance record auto-updates `Vehicle.InsuranceExpiryDate`
- Creating an inspection record auto-updates `Vehicle.TechInspectionExpiryDate`

### Authorization
Vehicle controllers use `[Authorize]` (authenticated users). The dispatcher role has read/write/delete permissions on all vehicle resources.

### Angular Patterns
- **Standalone components** with lazy loading
- **Signal-based state**: `signal()` for reactive state, `computed()` for derived values
- **DevExtreme forms**: `formItems()` signal pattern with `dx-form`
- **DevExtreme grids**: `dx-data-grid` with `dxi-column` and cell templates
- **Popups**: Use `<div class="popup-actions">` with `<dx-button>` for save/cancel (NOT `dxo-toolbar`/`dxi-item` — those don't render reliably)
- **Date handling**: Forms use plain `YYYY-MM-DD` strings. Date boxes use `dateSerializationFormat="yyyy-MM-dd"`. Edit popups extract dates via regex `iso.match(/^(\d{4})-(\d{2})-(\d{2})/)` and reformat as `YYYY-MM-DD` to avoid timezone shift.

### Route Ordering (Angular)
- Angular matches routes in order. Static path segments (e.g., `types`, `maintenance`) MUST come before parameterized segments (`:id`). Otherwise, `types` is captured as an `:id` value.

---

## Migration

EF Core migration: `20260619112152_AddVehicleManagement`
