import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of, map } from 'rxjs';
import {
  ResumeDto,
  ResumeSummaryDto,
  ResumeTemplateDescriptor,
  ResumeTemplateKey,
  ExportResumeOptions,
  ResumeImportResultDto,
  ResumeFormat,
} from '../models/resume.models';

@Injectable({
  providedIn: 'root',
})
export class ResumeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/resumes';

  readonly resumes = signal<ResumeSummaryDto[]>([]);
  readonly activeResume = signal<ResumeDto | null>(null);
  readonly templates = signal<ResumeTemplateDescriptor[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  readonly atsSafeTemplates = computed(() =>
    this.templates().filter((t) => t.isAtsSafe)
  );

  loadTemplates(): Observable<ResumeTemplateDescriptor[]> {
    return this.http.get<ResumeTemplateDescriptor[]>(`${this.baseUrl}/templates`).pipe(
      tap((data) => this.templates.set(data)),
      catchError((err) => {
        // Fallback default templates for offline/dev preview
        const fallback: ResumeTemplateDescriptor[] = [
          { key: ResumeTemplateKey.AtsFriendly, name: 'ATS Friendly', description: 'Single column clean layout', isAtsSafe: true, columns: 1, accentColor: '#000000', headingFont: 'Arial', bodyFont: 'Arial', baseFontSize: 10.5 },
          { key: ResumeTemplateKey.Professional, name: 'Professional', description: 'Serif headings accent', isAtsSafe: true, columns: 1, accentColor: '#1f3a5f', headingFont: 'Georgia', bodyFont: 'Calibri', baseFontSize: 10.5 },
          { key: ResumeTemplateKey.Executive, name: 'Executive', description: 'Prominent header layout', isAtsSafe: true, columns: 1, accentColor: '#5b4636', headingFont: 'Georgia', bodyFont: 'Georgia', baseFontSize: 11 },
          { key: ResumeTemplateKey.Minimal, name: 'Minimal', description: 'Plain clean typography', isAtsSafe: true, columns: 1, accentColor: '#333333', headingFont: 'Helvetica', bodyFont: 'Helvetica', baseFontSize: 10 },
          { key: ResumeTemplateKey.Modern, name: 'Modern', description: 'Two-column accent layout', isAtsSafe: false, columns: 2, accentColor: '#2563eb', headingFont: 'Verdana', bodyFont: 'Calibri', baseFontSize: 10 },
          { key: ResumeTemplateKey.Corporate, name: 'Corporate', description: 'Structured corporate style', isAtsSafe: true, columns: 1, accentColor: '#1e293b', headingFont: 'Times New Roman', bodyFont: 'Arial', baseFontSize: 10.5 },
        ];
        this.templates.set(fallback);
        return of(fallback);
      })
    );
  }

  loadResumes(): Observable<ResumeSummaryDto[]> {
    this.isLoading.set(true);
    return this.http.get<ResumeSummaryDto[]>(this.baseUrl).pipe(
      tap((data) => {
        this.resumes.set(data);
        this.isLoading.set(false);
      }),
      catchError((err) => {
        this.isLoading.set(false);
        this.error.set('Failed to load resumes.');
        return of([]);
      })
    );
  }

  loadResume(id: string): Observable<ResumeDto | null> {
    this.isLoading.set(true);
    return this.http.get<ResumeDto>(`${this.baseUrl}/${id}`).pipe(
      tap((data) => {
        this.activeResume.set(data);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        return of(null);
      })
    );
  }

  switchTemplate(resumeId: string, template: ResumeTemplateKey): Observable<ResumeDto> {
    return this.http
      .put<ResumeDto>(`${this.baseUrl}/${resumeId}/template`, { template })
      .pipe(
        tap((updated) => {
          if (this.activeResume()?.id === resumeId) {
            this.activeResume.set(updated);
          }
        })
      );
  }

  getPreviewHtml(resumeId: string, templateKey?: ResumeTemplateKey): Observable<string> {
    const params: any = {};
    if (templateKey !== undefined) params.templateKey = templateKey;
    return this.http.get(`${this.baseUrl}/${resumeId}/preview-html`, {
      params,
      responseType: 'text',
    });
  }

  exportResume(resumeId: string, options: ExportResumeOptions): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${resumeId}/export`, options, {
      responseType: 'blob',
    });
  }

  importResumes(files: File[]): Observable<ResumeImportResultDto> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file, file.name));
    return this.http.post<ResumeImportResultDto>(`${this.baseUrl}/import`, formData);
  }
}
