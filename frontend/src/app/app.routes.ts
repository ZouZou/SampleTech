import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },

  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  },

  {
    path: 'underwriter',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Underwriter'] },
    loadChildren: () => import('./features/underwriter/underwriter.routes').then(m => m.UNDERWRITER_ROUTES)
  },

  {
    path: 'agent',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Agent', 'Broker'] },
    loadChildren: () => import('./features/agent/agent.routes').then(m => m.AGENT_ROUTES)
  },

  {
    path: 'client',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Client'] },
    loadChildren: () => import('./features/client/client.routes').then(m => m.CLIENT_ROUTES)
  },

  { path: '**', redirectTo: '/auth/login' }
];
