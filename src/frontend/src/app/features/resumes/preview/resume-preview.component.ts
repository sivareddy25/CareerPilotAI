import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { forkJoin } from 'rxjs';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { IconButtonComponent } from '../../../shared/components/icon-button/icon-button.component';
import { SelectComponent, SelectOption } from '../../../shared/components/select/select.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ResumeRendererComponent } from '../renderer/resume-renderer.component';
import { ExportDialogComponent } from '../export/export-dialog.component';
import { ResumeService } from '../../../core/resumes/resume.service';
import { ResumeTemplateKey } from '../../../core/resumes/resume.models';
import { NotificationService } from '../../../core/services/notification.service';
import { toResumeErrorMessage } from '../resume-error';

/** Discrete zoom stops. A free-scrubbing slider is fiddly and rarely wanted. */
const ZOOM_LEVELS = [0.5, 0.65, 0.8, 1, 1.25, 1.5] as const;

/** A4 height in millimetres, used to draw page-break guides. */
const PAGE_HEIGHT_MM = 297;

@Component({
  selector: 'app-resume-preview',
  standalone: true,
  imports: [
    ButtonComponent,
    IconButtonComponent,
    SelectComponent,
    SpinnerComponent,
    BadgeComponent,
    ResumeRendererComponent,
    ExportDialogComponent,
  ],
  templateUrl: './resume-preview.component.html',
  styleUrl: './resume-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ResumePreviewComponent {
  private readonly resumes = inject(ResumeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly resume = this.resumes.resume;
  protected readonly templates = this.resumes.templates;
  protected readonly activeTemplate = this.resumes.activeTemplate;

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly exportOpen = signal(false);

  protected readonly zoom = signal(1);
  protected readonly showPageBreaks = signal(true);
  protected readonly pageHeightMm = PAGE_HEIGHT_MM;

  private readonly resumeId = toSignal(
    this.route.paramMap,
    { initialValue: this.route.snapshot.paramMap },
  );

  protected readonly zoomPercent = computed(() => Math.round(this.zoom() * 100));

  protected readonly templateOptions = computed<SelectOption[]>(() =>
    this.templates().map((template) => ({
      value: template.key,
      // The ATS warning belongs in the option itself: it is the single most
      // consequential thing about the choice and cannot be inferred from a name.
      label: template.isAtsSafe ? template.name : `${template.name} (not ATS-safe)`,
    })),
  );

  constructor() {
    const id = this.resumeId().get('resumeId');

    if (!id) {
      this.error.set('No resume was specified.');
      this.loading.set(false);
    } else {
      // Templates and resume are fetched together: the renderer needs a descriptor, and
      // rendering with a fallback first would flash the wrong layout.
      forkJoin({
        templates: this.resumes.loadTemplates(),
        resume: this.resumes.load(id),
      }).subscribe({
        next: () => this.loading.set(false),
        error: (error: unknown) => {
          this.loading.set(false);
          this.error.set(toResumeErrorMessage(error));
        },
      });
    }
  }

  protected zoomIn(): void {
    const next = ZOOM_LEVELS.find((level) => level > this.zoom());
    if (next) this.zoom.set(next);
  }

  protected zoomOut(): void {
    const previous = [...ZOOM_LEVELS].reverse().find((level) => level < this.zoom());
    if (previous) this.zoom.set(previous);
  }

  protected resetZoom(): void {
    this.zoom.set(1);
  }

  protected togglePageBreaks(): void {
    this.showPageBreaks.update((value) => !value);
  }

  /**
   * Switches template.
   *
   * Optimistic in the service, so the preview redraws on the same frame as the click —
   * which is what makes browsing templates feel like a gallery rather than a form.
   */
  protected switchTemplate(value: string | number): void {
    const resume = this.resume();
    if (!resume) return;

    this.resumes.switchTemplate(resume.id, Number(value) as ResumeTemplateKey).subscribe({
      error: (error: unknown) => this.notifications.error('Could not switch template', toResumeErrorMessage(error)),
    });
  }

  /**
   * Prints the live preview.
   *
   * Uses the browser's own print path rather than downloading a PDF first: what prints
   * is exactly what is on screen, so the two cannot disagree. Print CSS in the
   * stylesheet hides the surrounding chrome and resets the zoom, so a zoomed preview
   * still prints at true size.
   */
  protected print(): void {
    window.print();
  }

  protected openExport(): void {
    this.exportOpen.set(true);
  }

  protected backToList(): void {
    void this.router.navigate(['/resumes']);
  }
}
