import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';

export interface UnansweredQuestionPrompt {
  questionKey: string;
  questionText: string;
  fieldType?: string;
}

export interface AutoApplyResult {
  success: boolean;
  statusMessage: string;
  applicationUrl?: string;
  missingQuestions: UnansweredQuestionPrompt[];
}

export interface CandidateAnswer {
  id: string;
  questionKey: string;
  questionText: string;
  answerText: string;
  category: string;
  lastUsedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class AutomationService {
  private readonly http = inject(HttpClient);

  readonly isApplying = signal<boolean>(false);
  readonly answers = signal<CandidateAnswer[]>([]);
  readonly statusBanner = signal<{ type: 'success' | 'warning' | 'error'; message: string } | null>(null);

  executeAutoApply(jobId: string): Observable<AutoApplyResult> {
    this.isApplying.set(true);
    this.statusBanner.set(null);

    return this.http.post<AutoApplyResult>('/api/v1/automation/apply', { jobId }).pipe(
      tap((res) => {
        this.isApplying.set(false);
        if (res.success) {
          this.statusBanner.set({
            type: 'success',
            message: res.statusMessage || 'Playwright successfully populated candidate details and paused at the Human Approval Checkpoint.',
          });
        } else if (res.missingQuestions && res.missingQuestions.length > 0) {
          this.statusBanner.set({
            type: 'warning',
            message: 'Application form requires additional custom answers before final submission.',
          });
        } else {
          this.statusBanner.set({
            type: 'warning',
            message: res.statusMessage || 'Auto-apply completed with warnings.',
          });
        }
      }),
      catchError((err) => {
        this.isApplying.set(false);
        const errMsg = err.error?.detail || err.error?.message || 'Automation service connection error. Please verify backend API is running.';
        this.statusBanner.set({
          type: 'error',
          message: errMsg,
        });
        return of({
          success: false,
          statusMessage: errMsg,
          missingQuestions: [],
        });
      })
    );
  }

  getCandidateAnswers(): Observable<CandidateAnswer[]> {
    return this.http.get<CandidateAnswer[]>('/api/v1/automation/answers').pipe(
      tap((data) => this.answers.set(data)),
      catchError(() => of([]))
    );
  }

  saveCandidateAnswer(questionKey: string, questionText: string, answerText: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>('/api/v1/automation/answers', {
      questionKey,
      questionText,
      answerText,
      category: 'General',
    }).pipe(
      catchError(() => of({ id: 'local-saved' }))
    );
  }
}
