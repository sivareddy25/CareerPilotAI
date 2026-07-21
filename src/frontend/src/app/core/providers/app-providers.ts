import {
  EnvironmentProviders,
  ErrorHandler,
  Provider,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { routes } from '../../app.routes';
import { requestIdInterceptor } from '../interceptors/request-id.interceptor';
import { authenticationInterceptor } from '../interceptors/authentication.interceptor';
import { loadingInterceptor } from '../interceptors/loading.interceptor';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { GlobalErrorHandler } from '../error-handling/global-error-handler';
import { AuthenticationService } from '../authentication/authentication.service';

export function provideCoreInfrastructure(): Array<Provider | EnvironmentProviders> {
  return [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([
        requestIdInterceptor,
        loadingInterceptor,
        errorInterceptor,
        // Innermost, and deliberately last in this list. Interceptors nest in array
        // order, so the final entry sits closest to the backend and sees a 401 first —
        // letting it refresh and retry before errorInterceptor can surface the failure
        // to the user. Moving it earlier would make every silent refresh flash an
        // error notification.
        authenticationInterceptor,
      ]),
    ),
    // Session restoration. Bootstrap waits for this, so by the time the router runs a
    // guard the authentication state is settled — no route flashes the sign-in page
    // before a valid session is recognised.
    provideAppInitializer(() => {
      const auth = inject(AuthenticationService);
      return firstValueFrom(auth.restoreSession());
    }),
    {
      provide: ErrorHandler,
      useClass: GlobalErrorHandler,
    },
  ];
}
