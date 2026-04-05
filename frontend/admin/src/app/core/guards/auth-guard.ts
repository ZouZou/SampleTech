import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { getRolePortal } from './role-guard';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};

/** After verifying auth, redirect unauthenticated users to login
 *  and authenticated users at the root path to their role portal. */
export const rootRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const user = auth.currentUser();

  if (!user) {
    router.navigate(['/login']);
    return false;
  }

  router.navigate([getRolePortal(user.role)]);
  return false;
};
