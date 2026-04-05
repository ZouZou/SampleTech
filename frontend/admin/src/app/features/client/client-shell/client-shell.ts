import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-client-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './client-shell.html',
  styleUrl: './client-shell.scss',
})
export class ClientShell {
  auth = inject(AuthService);
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  navItems = [
    { label: 'Dashboard',      path: '/client/dashboard',  icon: '⊞', ariaLabel: 'Dashboard' },
    { label: 'My Policies',    path: '/client/policies',   icon: '📋', ariaLabel: 'My Policies' },
    { label: 'Support',        path: '/client/support',    icon: '✉', ariaLabel: 'Contact Support' },
  ];

  toggleSidebar() { this.sidebarOpen.update(v => !v); }
  toggleMobileMenu() { this.mobileMenuOpen.update(v => !v); }
}
