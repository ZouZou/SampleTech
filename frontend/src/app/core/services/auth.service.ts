import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type UserRole = 'Admin' | 'Underwriter' | 'Agent' | 'Broker' | 'Client';

export interface CurrentUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  tenantId: string | null;
}

export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  user: CurrentUser;
}

export interface LoginRequest {
  email: string;
  password: string;
}

const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/admin',
  Underwriter: '/underwriter',
  Agent: '/agent',
  Broker: '/agent',   // brokers share the agent portal
  Client: '/client',
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<CurrentUser | null>(
    this.loadStoredUser()
  );

  readonly isAuthenticated = computed(() => !!this._currentUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly currentRole = computed(() => this._currentUser()?.role ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<LoginResult> {
    return this.http.post<LoginResult>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(result => this.storeSession(result))
    );
  }

  refreshTokens(): Observable<LoginResult> {
    const refreshToken = localStorage.getItem('refresh_token');
    return this.http.post<LoginResult>(`${environment.apiUrl}/auth/refresh`, { refreshToken }).pipe(
      tap(result => this.storeSession(result))
    );
  }

  logout(): void {
    const refreshToken = localStorage.getItem('refresh_token');
    if (refreshToken) {
      // Fire-and-forget — don't block navigation on failure
      this.http.post(`${environment.apiUrl}/auth/logout`, { refreshToken }).subscribe({ error: () => {} });
    }
    this.clearSession();
    this.router.navigate(['/auth/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getRoleHome(): string {
    const role = this._currentUser()?.role;
    return role ? ROLE_HOME[role] : '/auth/login';
  }

  navigateToRoleHome(): void {
    this.router.navigate([this.getRoleHome()]);
  }

  private storeSession(result: LoginResult): void {
    localStorage.setItem('access_token', result.accessToken);
    localStorage.setItem('refresh_token', result.refreshToken);
    localStorage.setItem('current_user', JSON.stringify(result.user));
    this._currentUser.set(result.user);
  }

  private clearSession(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('current_user');
    this._currentUser.set(null);
  }

  private loadStoredUser(): CurrentUser | null {
    try {
      const raw = localStorage.getItem('current_user');
      return raw ? (JSON.parse(raw) as CurrentUser) : null;
    } catch {
      return null;
    }
  }
}
