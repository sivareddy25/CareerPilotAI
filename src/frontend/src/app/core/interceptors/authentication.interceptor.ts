import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { API_CONSTANTS } from '../constants/api.constants';
import { AuthenticationService } from '../authentication/authentication.service';
import { TokenStorageService } from '../authentication/token-storage.service';
import { HostingService } from '../services/hosting.service';

const ANONYMOUS_ENDPOINTS = ['/auth/login', '/auth/register', '/auth/refresh'];

function isAnonymous(request: HttpRequest<unknown>): boolean {
  return ANONYMOUS_ENDPOINTS.some((endpoint) => request.url.includes(endpoint));
}

function withBearer(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return request.clone({
    headers: request.headers.set(API_CONSTANTS.HEADERS.AUTHORIZATION, `Bearer ${token}`),
  });
}

export const authenticationInterceptor: HttpInterceptorFn = (req, next) => {
  const hosting = inject(HostingService);
  if (hosting.isLocalMode()) {
    return next(req);
  }

  const auth = inject(AuthenticationService);
  const tokens = inject(TokenStorageService);
  const router = inject(Router);

  if (isAnonymous(req)) {
    return next(req);
  }

  const failSession = (error: HttpErrorResponse) => {
    auth.clearSession();
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
      catchError((error: HttpErrorResponse) => failSession(error))
    );

  if (tokens.isAccessTokenExpired() && tokens.hasRefreshToken()) {
    return sendWithRefresh();
  }

  const accessToken = tokens.getAccessToken();
  const authorized = accessToken ? withBearer(req, accessToken) : req;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }

      if (!tokens.hasRefreshToken()) {
        return failSession(error);
      }

      return sendWithRefresh();
    })
  );
};
