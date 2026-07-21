import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { Observable, map, of, shareReplay, tap } from 'rxjs';
import { ApiService } from '../services/api.service';
import {
  Resume,
  ResumeFormat,
  ResumeImportResult,
  ResumeSummary,
  ResumeTemplateDescriptor,
  ResumeTemplateKey,
} from './resume.models';

/**
 * Resume data and the operations on it.
 *
 * State is signal-backed and shared: the preview, the gallery and the export dialog all
 * read the same loaded resume, so switching a template updates every one of them
 * without any of them talking to each other.
 */
@Injectable({
  providedIn: 'root',
})
export class ResumeService {
  private readonly api = inject(ApiService);

  private readonly currentResume = signal<Resume | null>(null);
  private readonly resumeList = signal<readonly ResumeSummary[]>([]);
  private readonly loading = signal(false);

  readonly resume = this.currentResume.asReadonly();
  readonly resumes = this.resumeList.asReadonly();
  readonly isLoading = this.loading.asReadonly();

  readonly document = computed(() => this.currentResume()?.document ?? null);

  /**
   * The template catalogue, fetched once.
   *
   * Templates are static presentation data shared by every screen, so the request is
   * cached with `shareReplay` — the gallery, the preview and the export dialog each ask
   * for it, and there is no reason to fetch it three times.
   */
  private templatesRequest?: Observable<readonly ResumeTemplateDescriptor[]>;

  private readonly templateCache = signal<readonly ResumeTemplateDescriptor[]>([]);
  readonly templates = this.templateCache.asReadonly();

  /** The descriptor for the loaded resume's template — what the renderer styles from. */
  readonly activeTemplate = computed<ResumeTemplateDescriptor | null>(() => {
    const resume = this.currentResume();
    if (!resume) return null;

    return this.templateCache().find((template) => template.key === resume.template) ?? null;
  });

  loadTemplates(): Observable<readonly ResumeTemplateDescriptor[]> {
    this.templatesRequest ??= this.api
      .get<ResumeTemplateDescriptor[]>('/resumes/templates')
      .pipe(
        tap((templates) => this.templateCache.set(templates)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.templatesRequest;
  }

  list(): Observable<readonly ResumeSummary[]> {
    this.loading.set(true);

    return this.api.get<ResumeSummary[]>('/resumes').pipe(
      tap({
        next: (items) => {
          this.resumeList.set(items);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  load(resumeId: string, force = false): Observable<Resume> {
    const cached = this.currentResume();

    if (!force && cached?.id === resumeId) {
      return of(cached);
    }

    this.loading.set(true);

    return this.api.get<Resume>(`/resumes/${resumeId}`).pipe(
      tap({
        next: (resume) => {
          this.currentResume.set(resume);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  /**
   * Switches template optimistically.
   *
   * Appropriate because the change is presentation-only and cannot be rejected for any
   * reason the client could have anticipated — the server touches nothing but one enum
   * column. The preview redraws instantly, which is the entire point of a gallery.
   */
  switchTemplate(resumeId: string, template: ResumeTemplateKey): Observable<Resume> {
    const previous = this.currentResume();

    if (previous?.id === resumeId) {
      this.currentResume.set({ ...previous, template });
    }

    return this.api
      .put<Resume, { template: ResumeTemplateKey }>(`/resumes/${resumeId}/template`, { template })
      .pipe(
        tap({
          next: (resume) => this.currentResume.set(resume),
          error: () => {
            if (previous) {
              this.currentResume.set(previous);
            }
          },
        }),
      );
  }

  rename(resumeId: string, title: string): Observable<Resume> {
    return this.api
      .put<Resume, { title: string }>(`/resumes/${resumeId}/title`, { title })
      .pipe(tap((resume) => this.currentResume.set(resume)));
  }

  import(files: readonly File[]): Observable<ResumeImportResult> {
    const form = new FormData();

    // Repeated field name, which is how IFormFileCollection binds on the server.
    for (const file of files) {
      form.append('files', file, file.name);
    }

    // Content-Type deliberately unset so the browser generates the multipart boundary.
    return this.api.post<ResumeImportResult, FormData>('/resumes/import', form);
  }

  /**
   * Downloads a rendered resume.
   *
   * Reads the file name from Content-Disposition rather than reconstructing it, so the
   * saved file matches exactly what the server named it — including the sanitisation it
   * applied to the user's title.
   */
  export(
    resumeId: string,
    format: ResumeFormat,
    template?: ResumeTemplateKey,
  ): Observable<{ blob: Blob; fileName: string }> {
    return this.api
      .request<HttpResponse<Blob>>('post', `/resumes/${resumeId}/export`, {
        body: { format, template: template ?? null },
        responseType: 'blob',
        observe: 'response',
      })
      .pipe(
        map((response) => ({
          blob: response.body as Blob,
          fileName:
            parseFileName(response.headers?.get('Content-Disposition') ?? '')
            ?? `resume.${extensionFor(format)}`,
        })),
      );
  }

  delete(resumeId: string): Observable<void> {
    return this.api.delete<void>(`/resumes/${resumeId}`).pipe(
      tap(() => {
        this.resumeList.update((items) => items.filter((item) => item.id !== resumeId));

        if (this.currentResume()?.id === resumeId) {
          this.currentResume.set(null);
        }
      }),
    );
  }

  clear(): void {
    this.currentResume.set(null);
    this.resumeList.set([]);
  }
}

/** Prefers the RFC 5987 `filename*` form, falling back to plain `filename`. */
function parseFileName(disposition: string): string | null {
  const encoded = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
  if (encoded?.[1]) {
    try {
      return decodeURIComponent(encoded[1]);
    } catch {
      // Malformed encoding: fall through to the plain form rather than throwing.
    }
  }

  return /filename="?([^";]+)"?/i.exec(disposition)?.[1] ?? null;
}

function extensionFor(format: ResumeFormat): string {
  switch (format) {
    case ResumeFormat.Pdf:
      return 'pdf';
    case ResumeFormat.Docx:
      return 'docx';
    default:
      return 'json';
  }
}
