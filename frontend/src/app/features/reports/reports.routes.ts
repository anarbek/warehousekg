import { Routes } from '@angular/router';
export const reportRoutes: Routes = [
  { path: '', loadComponent: () => import('./dashboard/dashboard').then(m => m.Dashboard) },
  { path: 'stock-levels', loadComponent: () => import('./stock-levels/stock-levels').then(m => m.StockLevels) },
  { path: 'movements', loadComponent: () => import('./movement-history/movement-history').then(m => m.MovementHistory) },
];
