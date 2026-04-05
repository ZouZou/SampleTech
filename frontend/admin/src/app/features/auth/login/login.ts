import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { getRolePortal } from '../../../core/guards/role-guard';

type DevRole = 'Admin' | 'Underwriter' | 'Agent' | 'Broker' | 'Client';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  error = signal<string | null>(null);
  loading = signal(false);

  readonly devRoles: { role: DevRole; label: string; email: string }[] = [
    { role: 'Admin',       label: 'Admin',       email: 'admin@sampletech.com' },
    { role: 'Underwriter', label: 'Underwriter', email: 'underwriter@sampletech.com' },
    { role: 'Agent',       label: 'Agent',        email: 'agent@sampletech.com' },
    { role: 'Broker',      label: 'Broker',       email: 'broker@sampletech.com' },
    { role: 'Client',      label: 'Client',       email: 'client@sampletech.com' },
  ];

  /** Dev shortcut: bypass HTTP and navigate directly to role portal */
  quickLogin(role: DevRole) {
    const mockUser = {
      id: `dev-${role.toLowerCase()}`,
      email: `${role.toLowerCase()}@sampletech.com`,
      firstName: 'Dev',
      lastName: role,
      role,
    };
    localStorage.setItem('access_token', 'dev-token');
    localStorage.setItem('user', JSON.stringify(mockUser));
    window.location.href = getRolePortal(role);
  }

  submit() {
    this.error.set(null);
    this.loading.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        const user = this.auth.currentUser();
        const portal = user ? getRolePortal(user.role) : '/login';
        this.router.navigate([portal]);
      },
      error: () => {
        this.error.set('Invalid email or password. Use the quick-login chips below for dev access.');
        this.loading.set(false);
      }
    });
  }
}
