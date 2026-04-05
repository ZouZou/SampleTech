import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

interface NavItem {
  label: string;
  path: string;
  icon: string;
  ariaLabel: string;
}

@Component({
  selector: 'app-admin-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-shell.html',
  styleUrl: './admin-shell.scss',
})
export class AdminShell {
  auth = inject(AuthService);
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  navItems: NavItem[] = [
    { label: 'Dashboard',       path: '/admin/dashboard',       icon: '⊞', ariaLabel: 'Dashboard' },
    { label: 'User Management', path: '/admin/users',           icon: '👥', ariaLabel: 'User Management' },
    { label: 'Tenant Config',   path: '/admin/tenant-config',   icon: '⚙', ariaLabel: 'Tenant Configuration' },
    { label: 'Audit Log',       path: '/admin/audit-log',       icon: '📋', ariaLabel: 'Audit Log' },
  ];

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update(v => !v);
  }
}
