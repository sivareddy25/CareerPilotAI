import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { ApiService } from '../services/api.service';
import { AuthenticationService } from '../authentication/authentication.service';
import { ThemeService } from '../services/theme.service';
import { ThemeMode } from '../state/theme.state';
import {
  ChangePasswordRequest,
  DeactivateAccountRequest,
  DeleteAccountRequest,
  Preferences,
  Profile,
  ProfileImage,
  ThemePreference,
  UpdateProfileRequest,
} from './profile.models';

/** Maps the persisted theme enum onto the values `ThemeService` understands. */
const THEME_TO_MODE: Record<ThemePreference, ThemeMode> = {
  [ThemePreference.System]: 'system',
  [ThemePreference.Light]: 'light',
  [ThemePreference.Dark]: 'dark',
  [ThemePreference.HighContrast]: 'high-contrast',
};

/**
 * Holds the caller's profile and the operations that change it.
 *
 * The loaded profile is cached in a signal and shared by every settings page, so
 * navigating between tabs does not refetch. Writes update that signal from the
 * server's response, which keeps the cache authoritative rather than guessed.
 */
@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthenticationService);
  private readonly theme = inject(ThemeService);

  private readonly current = signal<Profile | null>(null);
  private readonly loading = signal(false);

  readonly profile = this.current.asReadonly();
  readonly isLoading = this.loading.asReadonly();

  readonly preferences = computed<Preferences | null>(() => this.current()?.preferences ?? null);

  /**
   * What to show as the user's name, in decreasing order of preference. Falls back to
   * the authenticated identity so the shell has something to render before the profile
   * request resolves.
   */
  readonly displayName = computed(() => {
    const profile = this.current();
    if (!profile) return this.auth.displayName();

    const explicit = profile.displayName?.trim();
    if (explicit) return explicit;

    const full = [profile.firstName, profile.lastName].filter(Boolean).join(' ').trim();
    return full || profile.email;
  });

  readonly avatarUrl = computed(() => this.current()?.profilePictureUrl ?? undefined);

  /**
   * Fetches the profile, reusing the cached copy unless `force` is set.
   *
   * Guards on the cache rather than on an in-flight flag because the settings pages
   * each call this on init; without it, opening the section would fire one request per
   * tab visited.
   */
  load(force = false): Observable<Profile> {
    if (!force && this.current()) {
      return new Observable<Profile>((subscriber) => {
        subscriber.next(this.current()!);
        subscriber.complete();
      });
    }

    this.loading.set(true);

    return this.api.get<Profile>('/profile').pipe(
      tap({
        next: (profile) => {
          this.apply(profile);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  update(request: UpdateProfileRequest): Observable<Profile> {
    return this.api
      .put<Profile, UpdateProfileRequest>('/profile', request)
      .pipe(tap((profile) => this.apply(profile)));
  }

  /**
   * Saves preferences optimistically.
   *
   * Appropriate here and nowhere else in this module: a theme or notification toggle
   * has no server-side validation that can reject it, so the local state is already
   * what the server will store and waiting for the round-trip only makes the switch
   * feel broken. On failure the previous value is restored — including the applied
   * theme, so the UI cannot end up showing a setting that was never saved.
   *
   * Profile edits, password changes and account deletion are deliberately *not*
   * optimistic: each can legitimately fail, and pretending otherwise would show the
   * user a success they did not get.
   */
  updatePreferences(next: Preferences): Observable<Preferences> {
    const profile = this.current();
    const previous = profile?.preferences;

    if (profile) {
      this.apply({ ...profile, preferences: next });
    }

    return this.api.put<Preferences, Preferences>('/profile/preferences', next).pipe(
      tap((saved) => {
        const latest = this.current();
        if (latest) {
          this.apply({ ...latest, preferences: saved });
        }
      }),
      catchError((error: unknown) => {
        if (profile && previous) {
          this.apply({ ...profile, preferences: previous });
        }
        return throwError(() => error);
      }),
    );
  }

  uploadPicture(file: File): Observable<ProfileImage> {
    const form = new FormData();
    form.append('file', file, file.name);

    // Content-Type is left unset on purpose: the browser must generate it so the
    // multipart boundary is included. Setting it by hand produces a body the server
    // cannot parse.
    return this.api.post<ProfileImage, FormData>('/profile/picture', form).pipe(
      tap((result) => {
        const profile = this.current();
        if (profile) {
          this.apply({ ...profile, profilePictureUrl: result.profilePictureUrl });
        }
      }),
    );
  }

  deletePicture(): Observable<void> {
    return this.api.delete<void>('/profile/picture').pipe(
      tap(() => {
        const profile = this.current();
        if (profile) {
          this.apply({ ...profile, profilePictureUrl: null });
        }
      }),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    // The server revokes every session on success, including this one, so the local
    // session is cleared to match. Leaving it in place would leave the user clicking
    // through a UI whose every request now fails.
    return this.api
      .post<void, ChangePasswordRequest>('/profile/change-password', request)
      .pipe(tap(() => this.endSession()));
  }

  deactivate(request: DeactivateAccountRequest): Observable<void> {
    return this.api
      .post<void, DeactivateAccountRequest>('/profile/deactivate', request)
      .pipe(tap(() => this.endSession()));
  }

  deleteAccount(request: DeleteAccountRequest): Observable<void> {
    // DELETE with a body: the password must not travel in the URL.
    return this.api
      .request<void>('delete', '/profile', { body: request })
      .pipe(tap(() => this.endSession()));
  }

  /** Drops the cached profile — called on sign-out so the next user starts clean. */
  clear(): void {
    this.current.set(null);
  }

  private apply(profile: Profile): void {
    this.current.set(profile);

    // The stored theme is the user's account-level choice, so applying it on load is
    // what makes the preference follow them to a new device.
    this.theme.setTheme(THEME_TO_MODE[profile.preferences.theme] ?? 'system');
  }

  private endSession(): void {
    this.clear();
    this.auth.clearSession();
  }
}
