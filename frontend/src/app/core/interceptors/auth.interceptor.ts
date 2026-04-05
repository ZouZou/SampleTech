import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_ROUTES = ['/auth/login', '/auth/refresh', '/auth/forgot-password', '/auth/reset-password'];

/**
 * Attaches the Bearer token to all outgoing requests.
 * On 401 responses, attempts a single token refresh.
 * If the refresh also fails, clears the session and redirects to login.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Don't attach tokens to auth endpoints (avoids loops)
  const isAuthEndpoint = AUTH_ROUTES.some(path => req.url.includes(path));
  const token = authService.getAccessToken();

  const authedReq = token && !isAuthEndpoint
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isAuthEndpoint) {
        return handle401(req, next, authService);
      }
      return throwError(() => err);
    })
  );
};

function handle401(
  originalReq: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
) {
  return authService.refreshTokens().pipe(
    switchMap((result) => {
      // Retry the original request with the new access token
      const retried = originalReq.clone({
        setHeaders: { Authorization: `Bearer ${result.accessToken}` }
      });
      return next(retried);
    }),
    catchError((refreshErr) => {
      // Refresh failed — session is dead, send to login
      authService.logout();
      return throwError(() => refreshErr);
    })
  );
}
