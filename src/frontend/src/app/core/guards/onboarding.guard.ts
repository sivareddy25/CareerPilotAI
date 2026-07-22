import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { OnboardingService } from '../services/onboarding.service';

export const onboardingGuard: CanActivateFn = (_route, _state) => {
  const onboarding = inject(OnboardingService);
  const router = inject(Router);

  return onboarding.checkStatus().pipe(
    map((res) => {
      if (res.isCompleted) {
        return true;
      }
      return router.createUrlTree(['/onboarding']);
    }),
    catchError(() => of(router.createUrlTree(['/onboarding'])))
  );
};
