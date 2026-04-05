import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./admin-shell').then(m => m.AdminShell),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('../dashboard/admin-dashboard').then(m => m.AdminDashboard)
      },
      {
        path: 'users',
        loadComponent: () => import('../user-management/user-management').then(m => m.UserManagement)
      },
      {
        path: 'tenant-config',
        loadComponent: () => import('../tenant-config/tenant-config').then(m => m.TenantConfig)
      },
      {
        path: 'audit-log',
        loadComponent: () => import('../audit-log/audit-log').then(m => m.AuditLog)
      }
    ]
  }
];
