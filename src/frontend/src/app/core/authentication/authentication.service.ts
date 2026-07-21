import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, of, shareReplay, tap, throwError } from 'rxjs';
import { ApiService } from '../services/api.service';
import { TokenStorageService } from './token-storage.service';
import {
  AuthenticationResult,
  LoginRequest,
  RegisterRequest,
  UserProfile,
} from './authentication.models';

/**
 * Owns the client's authentication state and the calls that change it.
 *
 * State is exposed as signals so guards, the shell and any component read the same
 * source; nothing derives "am I signed in" from inspecting storage on its own.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private readonly api = inject(ApiService);
  private readonly tokens = inject(TokenStorageService);

  private readonly currentUser = signal<UserProfile | null>(null);
  private readonly restored = signal(false);

  /**
   * In-flight refresh, shared by every caller.
   *
   * Without this, several requests failing with 401 at once would each start their own
   * refresh. Since refresh tokens rotate, the first would succeed and invalidate the
   * token the others are still using — the server would see the rest as replays and
   * revoke the entire session. Collapsing them into one call is what makes rotation
   * and concurrency coexist.
   */
  private refreshInFlight: Observable<AuthenticationResult> | null = null;

  readonly user = this.currentUser.asReadonly();

  /** True once a session has been established or restoration has run and found none. */
  readonly isSessionRestored = this.restored.asReadonly();

  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  readonly roles = computed<readonly string[]>(() => this.currentUser()?.roles ?? []);

  readonly permissions = computed<readonly string[]>(() => this.currentUser()?.permissions ?? []);

  readonly displayName = computed(() => {
    const user = this.currentUser();
    if (!user) return '';

    const name = [user.firstName, user.lastName].filter(Boolean).join(' ').trim();
    return name || user.email;
  });

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  hasAnyPermission(permissions: readonly string[]): boolean {
    return permissions.some((permission) => this.hasPermission(permission));
  }

  login(request: LoginRequest): Observable<AuthenticationResult> {
    return this.api
      .post<AuthenticationResult, LoginRequest>('/auth/login', request)
      .pipe(tap((result) => this.applySession(result)));
  }

  register(request: RegisterRequest): Observable<AuthenticationResult> {
    return this.api
      .post<AuthenticationResult, RegisterRequest>('/auth/register', request)
      .pipe(tap((result) => this.applySession(result)));
  }

  /**
   * Rotates the refresh token for a new pair.
   *
   * Any failure clears local state: a refresh that does not succeed means the session
   * is gone server-side, and keeping a user "signed in" against a session that no
   * longer exists only produces a stream of failing requests.
   */
  refresh(): Observable<AuthenticationResult> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    const refreshToken = this.tokens.getRefreshToken();

    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    this.refreshInFlight = this.api
      .post<AuthenticationResult, { refreshToken: string }>('/auth/refresh', { refreshToken })
      .pipe(
        tap({
          next: (result) => {
            this.applySession(result);
            this.refreshInFlight = null;
          },
          error: () => {
            this.clearSession();
            this.refreshInFlight = null;
          },
        }),
        // Replays the single result to every subscriber that arrived while the call
        // was in flight, instead of re-issuing the request for each of them.
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.refreshInFlight;
  }

  /**
   * Signs out. Local state is cleared regardless of what the server replies, because
   * a network failure must not leave the browser holding usable credentials — the
   * server-side revocation can be retried, but the local wipe cannot be deferred.
   */
  logout(): Observable<void> {
    const refreshToken = this.tokens.getRefreshToken();

    return this.api
      .post<void, { refreshToken: string | null }>('/auth/logout', { refreshToken })
      .pipe(
        catchError(() => of(void 0)),
        tap(() => this.clearSession()),
      );
  }

  /**
   * Re-establishes a session on application start.
   *
   * Because the access token is memory-only, a page reload always begins with no
   * usable credential. Restoration is therefore a refresh: the persisted refresh token
   * is exchanged for a new pair. A failure here is expected and silent — an absent or
   * expired token simply means nobody is signed in.
   */
  restoreSession(): Observable<UserProfile | null> {
    if (!this.tokens.hasRefreshToken()) {
      this.restored.set(true);
      return of(null);
    }

    return new Observable<UserProfile | null>((subscriber) => {
      const subscription = this.refresh().subscribe({
        next: (result) => {
          this.restored.set(true);
          subscriber.next(result.user);
          subscriber.complete();
        },
        error: () => {
          this.clearSession();
          this.restored.set(true);
          subscriber.next(null);
          subscriber.complete();
        },
      });

      return () => subscription.unsubscribe();
    });
  }

  /** Called by the interceptor after a hard authentication failure. */
  clearSession(): void {
    this.tokens.clear();
    this.currentUser.set(null);
  }

  private applySession(result: AuthenticationResult): void {
    this.tokens.setAccessToken(result.accessToken, result.accessTokenExpiresAt);
    this.tokens.setRefreshToken(result.refreshToken);
    this.currentUser.set(result.user);
    this.restored.set(true);
  }
}
