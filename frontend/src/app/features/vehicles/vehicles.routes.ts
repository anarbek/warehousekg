import { Routes } from '@angular/router';

export const vehicleRoutes: Routes = [
  { path: '', redirectTo: 'list', pathMatch: 'full' },
  { path: 'list', loadComponent: () => import('./vehicle-list/vehicle-list').then(m => m.VehicleList) },
  { path: 'new', loadComponent: () => import('./vehicle-form/vehicle-form').then(m => m.VehicleForm) },
  { path: 'types', loadComponent: () => import('./vehicle-type-list/vehicle-type-list').then(m => m.VehicleTypeList) },
  { path: 'types/new', loadComponent: () => import('./vehicle-type-form/vehicle-type-form').then(m => m.VehicleTypeForm) },
  { path: 'types/:id/edit', loadComponent: () => import('./vehicle-type-form/vehicle-type-form').then(m => m.VehicleTypeForm) },
  { path: 'maintenance', loadComponent: () => import('./maintenance-list/maintenance-list').then(m => m.MaintenanceList) },
  { path: 'insurance', loadComponent: () => import('./insurance-list/insurance-list').then(m => m.InsuranceList) },
  { path: 'inspections', loadComponent: () => import('./inspection-list/inspection-list').then(m => m.InspectionList) },
  { path: ':id', loadComponent: () => import('./vehicle-detail/vehicle-detail').then(m => m.VehicleDetail) },
  { path: ':id/edit', loadComponent: () => import('./vehicle-form/vehicle-form').then(m => m.VehicleForm) },
];
