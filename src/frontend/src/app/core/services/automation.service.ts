import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

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

  executeAutoApply(jobId: string): Observable<AutoApplyResult> {
    this.isApplying.set(true);
    return this.http.post<AutoApplyResult>('/api/v1/automation/apply', { jobId }).pipe(
      tap(() => this.isApplying.set(false))
    );
  }

  getCandidateAnswers(): Observable<CandidateAnswer[]> {
    return this.http.get<CandidateAnswer[]>('/api/v1/automation/answers').pipe(
      tap((data) => this.answers.set(data))
    );
  }

  saveCandidateAnswer(questionKey: string, questionText: string, answerText: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>('/api/v1/automation/answers', {
      questionKey,
      questionText,
      answerText,
      category: 'General',
    });
  }
}
