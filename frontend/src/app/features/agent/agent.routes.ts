import { Routes } from '@angular/router';

export const AGENT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./agent-dashboard.component').then(m => m.AgentDashboardComponent)
  }
];
