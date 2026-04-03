import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  mfaRequired: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _accessToken = signal<string | null>(
    localStorage.getItem('access_token')
  );

  readonly isAuthenticated = computed(() => !!this._accessToken());

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(response => {
        if (!response.mfaRequired) {
          this._accessToken.set(response.accessToken);
          localStorage.setItem('access_token', response.accessToken);
        }
      })
    );
  }

  logout(): void {
    this._accessToken.set(null);
    localStorage.removeItem('access_token');
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return this._accessToken();
  }
}
