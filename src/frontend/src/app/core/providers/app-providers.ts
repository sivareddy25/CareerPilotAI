import { Provider, EnvironmentProviders, ErrorHandler, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from '../../app.routes';
import { requestIdInterceptor } from '../interceptors/request-id.interceptor';
import { authenticationInterceptor } from '../interceptors/authentication.interceptor';
import { loadingInterceptor } from '../interceptors/loading.interceptor';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { GlobalErrorHandler } from '../error-handling/global-error-handler';

export function provideCoreInfrastructure(): Array<Provider | EnvironmentProviders> {
  return [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([
        requestIdInterceptor,
        authenticationInterceptor,
        loadingInterceptor,
        errorInterceptor,
      ])
    ),
    {
      provide: ErrorHandler,
      useClass: GlobalErrorHandler,
    },
  ];
}
