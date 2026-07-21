import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '../authentication/authentication.service';

/**
 * Blocks routes that require a signed-in user.
 *
 * Session restoration has already completed by the time any guard runs — the app
 * initializer awaits it during bootstrap — so this only has to read the current state,
 * with no race against an in-flight refresh.
 *
 * This is a usability control, not a security one. It decides what the browser
 * renders; nothing here protects data. Every protected resource is enforced server
 * side by the JWT bearer handler and the authorization policies, which is the boundary
 * an attacker actually has to get past.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl: state.url },
  });
};
