import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '../authentication/authentication.service';
import { HostingService } from '../services/hosting.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const hosting = inject(HostingService);
  if (hosting.isLocalMode()) {
    return true;
  }

  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl: state.url },
  });
};
