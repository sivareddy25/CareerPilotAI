import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of, tap } from 'rxjs';
import { CareerProfileDto, EMPTY_CAREER_PROFILE } from './career-profile.models';

/**
 * The user's structured career profile — the input to job match scoring.
 *
 * Cached in a signal so the profile screen and any consumer share one copy; writes replace it
 * from the server's response, keeping the cache authoritative rather than optimistic.
 */
@Injectable({
  providedIn: 'root',
})
export class CareerProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/profile/career';

  private readonly current = signal<CareerProfileDto | null>(null);
  private readonly saving = signal(false);

  readonly profile = this.current.asReadonly();
  readonly isSaving = this.saving.asReadonly();

  load(): Observable<CareerProfileDto> {
    return this.http.get<CareerProfileDto>(this.baseUrl).pipe(
      tap((profile) => this.current.set(profile)),
      catchError(() => {
        this.current.set(EMPTY_CAREER_PROFILE);
        return of(EMPTY_CAREER_PROFILE);
      }),
    );
  }

  save(profile: CareerProfileDto): Observable<CareerProfileDto | null> {
    this.saving.set(true);
    return this.http.put<CareerProfileDto>(this.baseUrl, profile).pipe(
      tap((saved) => {
        this.current.set(saved);
        this.saving.set(false);
      }),
      catchError(() => {
        this.saving.set(false);
        return of(null);
      }),
    );
  }
}
