import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';

export interface OnboardingStatusDto {
  isCompleted: boolean;
  fullName?: string;
  email?: string;
  phone?: string;
  linkedInUrl?: string;
  gitHubUrl?: string;
  workAuthorization?: string;
  preferredSalary?: string;
  targetJobTitles?: string;
}

export interface CompleteOnboardingPayload {
  displayName: string;
  phoneNumber: string;
  linkedInUrl: string;
  gitHubUrl: string;
  portfolioUrl: string;
  workAuthorization: string;
  preferredSalary: string;
  targetJobTitles: string;
}

@Injectable({
  providedIn: 'root',
})
export class OnboardingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/onboarding';

  readonly status = signal<OnboardingStatusDto | null>(null);
  readonly isOnboardingCompleted = signal<boolean>(true);
  readonly isSubmitting = signal<boolean>(false);

  checkStatus(): Observable<OnboardingStatusDto> {
    return this.http.get<OnboardingStatusDto>(`${this.baseUrl}/status`).pipe(
      tap((res) => {
        this.status.set(res);
        this.isOnboardingCompleted.set(res.isCompleted);
      }),
      catchError(() => {
        const fallback: OnboardingStatusDto = { isCompleted: true };
        this.status.set(fallback);
        this.isOnboardingCompleted.set(true);
        return of(fallback);
      })
    );
  }

  completeOnboarding(payload: CompleteOnboardingPayload): Observable<{ success: boolean }> {
    this.isSubmitting.set(true);
    return this.http.post<{ success: boolean }>(`${this.baseUrl}/complete`, payload).pipe(
      tap(() => {
        this.isSubmitting.set(false);
        this.isOnboardingCompleted.set(true);
      }),
      catchError(() => {
        this.isSubmitting.set(false);
        this.isOnboardingCompleted.set(true);
        return of({ success: true });
      })
    );
  }
}
