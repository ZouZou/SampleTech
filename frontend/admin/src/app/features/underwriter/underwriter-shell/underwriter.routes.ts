import { Routes } from '@angular/router';

export const underwriterRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./underwriter-shell').then(m => m.UnderwriterShell),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('../dashboard/underwriter-dashboard').then(m => m.UnderwriterDashboard)
      },
      {
        path: 'queue',
        loadComponent: () => import('../submission-queue/submission-queue').then(m => m.SubmissionQueue)
      },
      {
        path: 'quotes',
        loadComponent: () => import('../quote-detail/quote-detail').then(m => m.QuoteDetail)
      },
      {
        path: 'quotes/:id',
        loadComponent: () => import('../quote-detail/quote-detail').then(m => m.QuoteDetail)
      }
    ]
  }
];
