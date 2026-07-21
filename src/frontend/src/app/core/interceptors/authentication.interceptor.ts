import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { API_CONSTANTS } from '../constants/api.constants';
import { AuthenticationService } from '../authentication/authentication.service';
import { TokenStorageService } from '../authentication/token-storage.service';

/**
 * Endpoints that must never carry a bearer token or trigger a refresh.
 *
 * Sign-in, registration and refresh are how a session is obtained in the first place.
 * Attaching an expired token to them is pointless, and letting a 401 from
 * `/auth/refresh` start another refresh would recurse until the stack gives out.
 */
const ANONYMOUS_ENDPOINTS = ['/auth/login', '/auth/register', '/auth/refresh'];

function isAnonymous(request: HttpRequest<unknown>): boolean {
  return ANONYMOUS_ENDPOINTS.some((endpoint) => request.url.includes(endpoint));
}

function withBearer(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return request.clone({
    headers: request.headers.set(API_CONSTANTS.HEADERS.AUTHORIZATION, `Bearer ${token}`),
  });
}

/**
 * Attaches the access token and performs silent refresh on expiry.
 *
 * Two paths lead to a refresh:
 *
 * 1. **Proactive** — the stored token is already past its skew window, so the request
 *    waits for a fresh one rather than being sent to certain rejection.
 * 2. **Reactive** — the server returns 401 on a token that looked valid. This covers
 *    revocation and clock drift, which the client cannot predict.
 *
 * Concurrency is handled in `AuthenticationService.refresh()`, which collapses
 * simultaneous callers into one request. That matters here: refresh tokens rotate, so
 * parallel refreshes would invalidate each other and look like token reuse to the API.
 *
 * This interceptor is registered **innermost**, so it sees a 401 before the error
 * interceptor does and can resolve it silently — a successful retry never surfaces as
 * a user-visible error.
 */
export const authenticationInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthenticationService);
  const tokens = inject(TokenStorageService);
  const router = inject(Router);

  if (isAnonymous(req)) {
    return next(req);
  }

  const failSession = (error: HttpErrorResponse) => {
    auth.clearSession();

    // returnUrl lets the user land back where they were after signing in again.
    void router.navigate(['/auth/login'], {
      queryParams: { returnUrl: router.url },
    });

    return throwError(() => error);
  };

  const sendWithRefresh = () =>
    auth.refresh().pipe(
      switchMap(() => {
        const refreshed = tokens.getAccessToken();
        return next(refreshed ? withBearer(req, refreshed) : req);
      }),
      catchError((error: HttpErrorResponse) => failSession(error)),
    );

  // Proactive: expired or about to expire, and a refresh token is available.
  if (tokens.isAccessTokenExpired() && tokens.hasRefreshToken()) {
    return sendWithRefresh();
  }

  const accessToken = tokens.getAccessToken();
  const authorized = accessToken ? withBearer(req, accessToken) : req;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only 401 means "credential problem". A 403 is a valid identity without
      // sufficient rights — refreshing would return the same answer, so it is left to
      // propagate.
      if (error.status !== 401) {
        return throwError(() => error);
      }

      if (!tokens.hasRefreshToken()) {
        return failSession(error);
      }

      return sendWithRefresh();
    }),
  );
};
