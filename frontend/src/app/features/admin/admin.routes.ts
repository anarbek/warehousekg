import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  { path: '', redirectTo: 'users', pathMatch: 'full' },
  {
    path: 'users',
    loadComponent: () => import('./users/users').then(m => m.UsersComponent),
  },
  {
    path: 'permissions',
    loadComponent: () => import('./permissions/permissions').then(m => m.Permissions),
  },
];
