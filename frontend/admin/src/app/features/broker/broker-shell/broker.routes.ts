import { Routes } from '@angular/router';

export const brokerRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./broker-shell').then(m => m.BrokerShell),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('../dashboard/broker-dashboard').then(m => m.BrokerDashboard)
      },
      {
        path: 'agencies',
        loadComponent: () => import('../agency-overview/agency-overview').then(m => m.AgencyOverview)
      },
      {
        path: 'portfolio',
        loadComponent: () => import('../portfolio-dashboard/portfolio-dashboard').then(m => m.PortfolioDashboard)
      },
      {
        path: 'commissions',
        loadComponent: () => import('../commission-summary/commission-summary').then(m => m.CommissionSummary)
      }
    ]
  }
];
