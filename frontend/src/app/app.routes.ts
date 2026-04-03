import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/admin', pathMatch: 'full' },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  },
  {
    path: 'underwriter',
    canActivate: [authGuard],
    loadChildren: () => import('./features/underwriter/underwriter.routes').then(m => m.UNDERWRITER_ROUTES)
  },
  {
    path: 'agent',
    canActivate: [authGuard],
    loadChildren: () => import('./features/agent/agent.routes').then(m => m.AGENT_ROUTES)
  },
  {
    path: 'client',
    canActivate: [authGuard],
    loadChildren: () => import('./features/client/client.routes').then(m => m.CLIENT_ROUTES)
  },
  { path: '**', redirectTo: '/admin' }
];
