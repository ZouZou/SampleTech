import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRole, AuthService } from '../services/auth.service';

/**
 * Route guard that checks the current user's role against the allowed roles
 * specified in the route data: `{ data: { roles: ['Admin', 'Underwriter'] } }`.
 *
 * Unauthenticated users are redirected to /auth/login.
 * Authenticated users with the wrong role are redirected to their own home.
 */
export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  const allowedRoles = route.data?.['roles'] as UserRole[] | undefined;
  if (!allowedRoles || allowedRoles.length === 0) {
    // No role restriction — any authenticated user may pass
    return true;
  }

  const userRole = authService.currentRole();
  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  // Redirect to the user's own home rather than showing a blank/error page
  return router.createUrlTree([authService.getRoleHome()]);
};
