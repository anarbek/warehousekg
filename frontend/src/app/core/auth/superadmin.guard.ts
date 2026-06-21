import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Route guard that only allows users with the Superadmin role.
 * Non-superadmin users are redirected to the dashboard.
 */
export const superadminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const roles = auth.currentUser()?.roles ?? [];
  if (roles.includes('Superadmin')) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
