import { Routes } from '@angular/router';

export const agentRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./agent-shell').then(m => m.AgentShell),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('../dashboard/agent-dashboard').then(m => m.AgentDashboard)
      },
      {
        path: 'clients',
        loadComponent: () => import('../client-list/client-list').then(m => m.ClientList)
      },
      {
        path: 'new-quote',
        loadComponent: () => import('../quote-submission/quote-submission').then(m => m.QuoteSubmission)
      },
      {
        path: 'status',
        loadComponent: () => import('../quote-status/quote-status').then(m => m.QuoteStatus)
      }
    ]
  }
];
