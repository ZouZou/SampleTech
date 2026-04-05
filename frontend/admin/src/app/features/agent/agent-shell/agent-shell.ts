import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-agent-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './agent-shell.html',
  styleUrl: './agent-shell.scss',
})
export class AgentShell {
  auth = inject(AuthService);
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  navItems = [
    { label: 'Dashboard',        path: '/agent/dashboard',   icon: '⊞', ariaLabel: 'Dashboard' },
    { label: 'Client List',      path: '/agent/clients',     icon: '👤', ariaLabel: 'Client List' },
    { label: 'New Quote',        path: '/agent/new-quote',   icon: '✚', ariaLabel: 'Submit New Quote' },
    { label: 'Quote Status',     path: '/agent/status',      icon: '📊', ariaLabel: 'Quote Status Tracker' },
  ];

  toggleSidebar() { this.sidebarOpen.update(v => !v); }
  toggleMobileMenu() { this.mobileMenuOpen.update(v => !v); }
}
