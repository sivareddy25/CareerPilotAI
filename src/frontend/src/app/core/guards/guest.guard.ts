import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '../authentication/authentication.service';
import { APP_CONSTANTS } from '../constants/app.constants';

/**
 * Keeps an already-signed-in user off the sign-in and registration pages, which would
 * otherwise offer to start a session they already have.
 */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree([APP_CONSTANTS.ROUTING.HOME]);
};
