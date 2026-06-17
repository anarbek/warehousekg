import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  { path: '', redirectTo: 'permissions', pathMatch: 'full' },
  {
    path: 'permissions',
    loadComponent: () => import('./permissions/permissions').then(m => m.Permissions),
  },
];
