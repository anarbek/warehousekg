import { Routes } from '@angular/router';

export const personnelRoutes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'positions', loadComponent: () => import('./positions/position-list/position-list').then(m => m.PositionList) },
  { path: 'positions/new', loadComponent: () => import('./positions/position-form/position-form').then(m => m.PositionForm) },
  { path: 'positions/:id/edit', loadComponent: () => import('./positions/position-form/position-form').then(m => m.PositionForm) },
  { path: 'departments', loadComponent: () => import('./departments/department-list/department-list').then(m => m.DepartmentList) },
  { path: 'departments/new', loadComponent: () => import('./departments/department-form/department-form').then(m => m.DepartmentForm) },
  { path: 'departments/:id/edit', loadComponent: () => import('./departments/department-form/department-form').then(m => m.DepartmentForm) },
  { path: 'employees', loadComponent: () => import('./employees/employee-list/employee-list').then(m => m.EmployeeList) },
  { path: 'employees/new', loadComponent: () => import('./employees/employee-form/employee-form').then(m => m.EmployeeForm) },
  { path: 'employees/:id', loadComponent: () => import('./employees/employee-detail/employee-detail').then(m => m.EmployeeDetail) },
  { path: 'employees/:id/edit', loadComponent: () => import('./employees/employee-form/employee-form').then(m => m.EmployeeForm) },
  { path: 'shifts', loadComponent: () => import('./shifts/shift-list/shift-list').then(m => m.ShiftList) },
  { path: 'shifts/new', loadComponent: () => import('./shifts/shift-form/shift-form').then(m => m.ShiftForm) },
  { path: 'shifts/:id/edit', loadComponent: () => import('./shifts/shift-form/shift-form').then(m => m.ShiftForm) },
  { path: 'attendance', loadComponent: () => import('./attendance/attendance-list/attendance-list').then(m => m.AttendanceList) },
  { path: 'attendance/new', loadComponent: () => import('./attendance/attendance-form/attendance-form').then(m => m.AttendanceForm) },
  { path: 'attendance/:id/edit', loadComponent: () => import('./attendance/attendance-form/attendance-form').then(m => m.AttendanceForm) },
];
