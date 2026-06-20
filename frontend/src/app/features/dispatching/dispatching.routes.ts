import { Routes } from '@angular/router';

export const dispatchingRoutes: Routes = [
  { path: '', redirectTo: 'routes', pathMatch: 'full' },
  { path: 'routes', loadComponent: () => import('./route-list/route-list').then(m => m.RouteList) },
  { path: 'routes/new', loadComponent: () => import('./route-form/route-form').then(m => m.RouteForm) },
  { path: 'routes/:id', loadComponent: () => import('./route-detail/route-detail').then(m => m.RouteDetail) },
  { path: 'routes/:id/edit', loadComponent: () => import('./route-form/route-form').then(m => m.RouteForm) },
  { path: 'geofences', loadComponent: () => import('./geofence-list/geofence-list').then(m => m.GeofenceList) },
];
