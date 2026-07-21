import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '../authentication/authentication.service';

/**
 * Guard factory for permission-gated routes:
 *
 * ```ts
 * { path: 'users', canActivate: [permissionGuard('users.read')], ... }
 * ```
 *
 * Requires *any* of the listed permissions, not all — the common case is one screen
 * reachable through several capabilities.
 *
 * As with `authGuard`, this shapes navigation only. The API re-checks the same
 * permission on every request the screen makes, so a user who edits their way past
 * this guard reaches a page whose data calls all return 403.
 */
export function permissionGuard(...permissions: readonly string[]): CanActivateFn {
  return () => {
    const auth = inject(AuthenticationService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/auth/login']);
    }

    if (auth.hasAnyPermission(permissions)) {
      return true;
    }

    // Not-found rather than a dedicated "forbidden" page: confirming that a route
    // exists but is barred tells an unprivileged user what the application contains.
    return router.createUrlTree(['/not-found']);
  };
}

/**
 * Role-gated variant. Prefer {@link permissionGuard} — roles are bundles that change,
 * whereas a permission names the capability the screen actually needs.
 */
export function roleGuard(...roles: readonly string[]): CanActivateFn {
  return () => {
    const auth = inject(AuthenticationService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/auth/login']);
    }

    if (roles.some((role) => auth.hasRole(role))) {
      return true;
    }

    return router.createUrlTree(['/not-found']);
  };
}
