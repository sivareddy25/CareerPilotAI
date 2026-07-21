import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { ResumeService } from '../../../core/resumes/resume.service';
import { IMPORT_ACCEPT, ResumeImportResult } from '../../../core/resumes/resume.models';
import { NotificationService } from '../../../core/services/notification.service';
import { toResumeErrorMessage } from '../resume-error';

/** Mirrors the server's per-file limit so oversized files are caught before upload. */
const MAX_FILE_BYTES = 5 * 1024 * 1024;
const MAX_FILES = 10;

type WizardStep = 'select' | 'review';

@Component({
  selector: 'app-import-wizard',
  standalone: true,
  imports: [ButtonComponent, CardComponent, IconComponent, PageHeaderComponent],
  templateUrl: './import-wizard.component.html',
  styleUrl: './import-wizard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ImportWizardComponent {
  private readonly resumes = inject(ResumeService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly accept = IMPORT_ACCEPT;
  protected readonly maxFiles = MAX_FILES;

  protected readonly step = signal<WizardStep>('select');
  protected readonly selected = signal<readonly File[]>([]);
  protected readonly rejected = signal<readonly { name: string; reason: string }[]>([]);
  protected readonly uploading = signal(false);
  protected readonly result = signal<ResumeImportResult | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly dragging = signal(false);

  protected readonly canUpload = computed(() => this.selected().length > 0 && !this.uploading());

  /** Files that produced a resume, so the review step can offer to open one. */
  protected readonly successes = computed(() =>
    this.result()?.results.filter((entry) => entry.succeeded) ?? [],
  );

  protected readonly failures = computed(() =>
    this.result()?.results.filter((entry) => !entry.succeeded) ?? [],
  );

  protected onFilesChosen(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addFiles(Array.from(input.files ?? []));
    // Reset so re-choosing the same file still fires a change event.
    input.value = '';
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    this.addFiles(Array.from(event.dataTransfer?.files ?? []));
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  protected onDragLeave(): void {
    this.dragging.set(false);
  }

  /**
   * Adds files, screening them client-side first.
   *
   * These checks mirror the server's and exist purely to save a round-trip — the server
   * repeats every one of them, and additionally sniffs the file's real content, which
   * the browser cannot do. Nothing here is an enforcement point.
   */
  private addFiles(incoming: File[]): void {
    const accepted: File[] = [...this.selected()];
    const refused: { name: string; reason: string }[] = [];

    for (const file of incoming) {
      if (accepted.length >= MAX_FILES) {
        refused.push({ name: file.name, reason: `Only ${MAX_FILES} files can be imported at once.` });
        continue;
      }

      if (file.size > MAX_FILE_BYTES) {
        refused.push({ name: file.name, reason: 'Larger than 5 MB.' });
        continue;
      }

      if (file.size === 0) {
        refused.push({ name: file.name, reason: 'The file is empty.' });
        continue;
      }

      const extension = file.name.split('.').pop()?.toLowerCase();
      if (!['pdf', 'docx', 'json'].includes(extension ?? '')) {
        refused.push({ name: file.name, reason: 'Only PDF, DOCX and JSON are supported.' });
        continue;
      }

      // De-duplicated by name and size: dropping the same file twice is a common slip.
      if (accepted.some((existing) => existing.name === file.name && existing.size === file.size)) {
        continue;
      }

      accepted.push(file);
    }

    this.selected.set(accepted);
    this.rejected.set(refused);
  }

  protected remove(file: File): void {
    this.selected.update((files) => files.filter((existing) => existing !== file));
  }

  protected upload(): void {
    if (!this.canUpload()) return;

    this.uploading.set(true);
    this.errorMessage.set(null);

    this.resumes.import(this.selected()).subscribe({
      next: (result) => {
        this.uploading.set(false);
        this.result.set(result);
        this.step.set('review');

        if (result.importedCount > 0) {
          this.notifications.success(
            `Imported ${result.importedCount} of ${result.totalFiles}`,
          );
        }
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        this.errorMessage.set(toResumeErrorMessage(error));
      },
    });
  }

  protected startOver(): void {
    this.selected.set([]);
    this.rejected.set([]);
    this.result.set(null);
    this.errorMessage.set(null);
    this.step.set('select');
  }

  protected openResume(resumeId: string | null): void {
    if (resumeId) {
      void this.router.navigate(['/resumes', resumeId]);
    }
  }

  protected goToList(): void {
    void this.router.navigate(['/resumes']);
  }

  protected formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;

    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
