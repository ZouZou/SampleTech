import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService, AuthUser } from '../services/auth';

export type AppRole = AuthUser['role'];

export function roleGuard(allowedRoles: AppRole[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const user = auth.currentUser();

    if (!user) {
      router.navigate(['/login']);
      return false;
    }

    if (allowedRoles.includes(user.role)) {
      return true;
    }

    // Redirect to correct portal for this user's role
    router.navigate([getRolePortal(user.role)]);
    return false;
  };
}

export function getRolePortal(role: AppRole): string {
  const portals: Record<AppRole, string> = {
    Admin:       '/admin',
    Underwriter: '/underwriter',
    Agent:       '/agent',
    Broker:      '/broker',
    Client:      '/client',
  };
  return portals[role] ?? '/login';
}
