import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-underwriter-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './underwriter-shell.html',
  styleUrl: './underwriter-shell.scss',
})
export class UnderwriterShell {
  auth = inject(AuthService);
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  navItems = [
    { label: 'Dashboard',         path: '/underwriter/dashboard',   icon: '⊞', ariaLabel: 'Dashboard' },
    { label: 'Submission Queue',  path: '/underwriter/queue',        icon: '📥', ariaLabel: 'Submission Queue' },
    { label: 'Quote Detail',      path: '/underwriter/quotes',       icon: '📄', ariaLabel: 'Quote Detail' },
  ];

  toggleSidebar() { this.sidebarOpen.update(v => !v); }
  toggleMobileMenu() { this.mobileMenuOpen.update(v => !v); }
}
