import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-broker-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './broker-shell.html',
  styleUrl: './broker-shell.scss',
})
export class BrokerShell {
  auth = inject(AuthService);
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  navItems = [
    { label: 'Dashboard',          path: '/broker/dashboard',   icon: '⊞', ariaLabel: 'Dashboard' },
    { label: 'Agency Overview',    path: '/broker/agencies',    icon: '🏢', ariaLabel: 'Agency Overview' },
    { label: 'Portfolio',          path: '/broker/portfolio',   icon: '📈', ariaLabel: 'Portfolio Dashboard' },
    { label: 'Commissions',        path: '/broker/commissions', icon: '💰', ariaLabel: 'Commission Summary' },
  ];

  toggleSidebar() { this.sidebarOpen.update(v => !v); }
  toggleMobileMenu() { this.mobileMenuOpen.update(v => !v); }
}
