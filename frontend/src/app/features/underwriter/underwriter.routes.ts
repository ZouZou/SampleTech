import { Routes } from '@angular/router';

export const UNDERWRITER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./underwriter-dashboard.component').then(m => m.UnderwriterDashboardComponent)
  }
];
