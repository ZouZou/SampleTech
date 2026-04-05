import { Routes } from '@angular/router';

export const clientRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./client-shell').then(m => m.ClientShell),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('../dashboard/client-dashboard').then(m => m.ClientDashboard)
      },
      {
        path: 'policies',
        loadComponent: () => import('../policy-list/policy-list').then(m => m.PolicyList)
      },
      {
        path: 'policies/:id',
        loadComponent: () => import('../policy-detail/policy-detail').then(m => m.PolicyDetail)
      },
      {
        path: 'support',
        loadComponent: () => import('../support-contact/support-contact').then(m => m.SupportContact)
      }
    ]
  }
];
