import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { TemplateGalleryComponent } from '../gallery/template-gallery.component';
import { ResumeService } from '../../../core/resumes/resume.service';
import { ResumeFormat, ResumeSummary, ResumeTemplateKey } from '../../../core/resumes/resume.models';
import { NotificationService } from '../../../core/services/notification.service';
import { toResumeErrorMessage } from '../resume-error';

@Component({
  selector: 'app-resume-list',
  standalone: true,
  imports: [
    RouterLink,
    ButtonComponent,
    CardComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    SkeletonComponent,
    BadgeComponent,
    TemplateGalleryComponent,
  ],
  templateUrl: './resume-list.component.html',
  styleUrl: './resume-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ResumeListComponent {
  private readonly resumes = inject(ResumeService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly items = this.resumes.resumes;
  protected readonly templates = this.resumes.templates;
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  /** Resume whose template is being changed from the gallery, or null when closed. */
  protected readonly galleryFor = signal<ResumeSummary | null>(null);
  protected readonly galleryDocument = this.resumes.document;

  constructor() {
    forkJoin({
      templates: this.resumes.loadTemplates(),
      list: this.resumes.list(),
    }).subscribe({
      next: () => this.loading.set(false),
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(toResumeErrorMessage(error));
      },
    });
  }

  protected templateName(key: ResumeTemplateKey): string {
    return this.templates().find((template) => template.key === key)?.name ?? 'Template';
  }

  protected isAtsSafe(key: ResumeTemplateKey): boolean {
    return this.templates().find((template) => template.key === key)?.isAtsSafe ?? true;
  }

  protected sourceLabel(format: ResumeFormat | null): string | null {
    switch (format) {
      case ResumeFormat.Pdf:
        return 'From PDF';
      case ResumeFormat.Docx:
        return 'From Word';
      case ResumeFormat.Json:
        return 'From JSON';
      default:
        return null;
    }
  }

  protected open(resumeId: string): void {
    void this.router.navigate(['/resumes', resumeId]);
  }

  /**
   * Opens the gallery for a resume, loading its document first.
   *
   * The gallery renders each template with the user's own content, so it needs the full
   * document — which the list deliberately does not carry.
   */
  protected openGallery(resume: ResumeSummary): void {
    this.resumes.load(resume.id).subscribe({
      next: () => this.galleryFor.set(resume),
      error: (error: unknown) => this.notifications.error('Could not open templates', toResumeErrorMessage(error)),
    });
  }

  protected chooseTemplate(key: ResumeTemplateKey): void {
    const resume = this.galleryFor();
    if (!resume) return;

    this.resumes.switchTemplate(resume.id, key).subscribe({
      next: () => {
        this.notifications.success('Template updated');
        this.galleryFor.set(null);
        this.resumes.list().subscribe();
      },
      error: (error: unknown) =>
        this.notifications.error('Could not switch template', toResumeErrorMessage(error)),
    });
  }

  protected remove(resume: ResumeSummary): void {
    this.resumes.delete(resume.id).subscribe({
      next: () => this.notifications.success('Resume deleted'),
      error: (error: unknown) => this.notifications.error('Could not delete', toResumeErrorMessage(error)),
    });
  }
}
