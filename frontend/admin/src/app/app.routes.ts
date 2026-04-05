import { Routes } from '@angular/router';
import { authGuard, rootRedirectGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  // Public routes
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },

  // Root redirect — sends authenticated users to their portal
  {
    path: '',
    canActivate: [rootRedirectGuard],
    children: []
  },

  // --- Admin Portal ---
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin-shell/admin.routes').then(m => m.adminRoutes),
    canActivate: [authGuard, roleGuard(['Admin'])]
  },

  // --- Underwriter Portal ---
  {
    path: 'underwriter',
    loadChildren: () => import('./features/underwriter/underwriter-shell/underwriter.routes').then(m => m.underwriterRoutes),
    canActivate: [authGuard, roleGuard(['Underwriter'])]
  },

  // --- Agent Portal ---
  {
    path: 'agent',
    loadChildren: () => import('./features/agent/agent-shell/agent.routes').then(m => m.agentRoutes),
    canActivate: [authGuard, roleGuard(['Agent'])]
  },

  // --- Broker Portal ---
  {
    path: 'broker',
    loadChildren: () => import('./features/broker/broker-shell/broker.routes').then(m => m.brokerRoutes),
    canActivate: [authGuard, roleGuard(['Broker'])]
  },

  // --- Client Portal ---
  {
    path: 'client',
    loadChildren: () => import('./features/client/client-shell/client.routes').then(m => m.clientRoutes),
    canActivate: [authGuard, roleGuard(['Client'])]
  },

  // Wildcard
  { path: '**', redirectTo: '' }
];
