import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { SelectComponent, SelectOption } from '../../../shared/components/select/select.component';
import { FocusTrapDirective } from '../../../shared/directives/focus-trap.directive';
import { ResumeService } from '../../../core/resumes/resume.service';
import {
  EXPORT_FORMATS,
  Resume,
  ResumeFormat,
  ResumeTemplateDescriptor,
  ResumeTemplateKey,
} from '../../../core/resumes/resume.models';
import { NotificationService } from '../../../core/services/notification.service';
import { readBlobError } from '../resume-error';

/**
 * Format and template picker for downloading a resume.
 *
 * The template choice here is an override: it renders the download in a different look
 * without changing the template the resume is stored with. Exporting a PDF in Executive
 * for one application should not silently restyle the resume for the next.
 */
@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [ButtonComponent, SelectComponent, FocusTrapDirective],
  template: `
    <div class="dialog-backdrop" (click)="close()">
      <!--
        stopPropagation so a click inside the panel does not reach the backdrop and
        dismiss the dialog the user is filling in.
      -->
      <div
        class="dialog-panel"
        role="dialog"
        aria-modal="true"
        aria-labelledby="export-title"
        appFocusTrap
        (click)="$event.stopPropagation()"
        (keydown.escape)="close()"
      >
        <h2 id="export-title" class="dialog-title">Export resume</h2>
        <p class="dialog-subtitle">{{ resume().title }}</p>

        @if (errorMessage(); as message) {
          <p class="dialog-error" role="alert">{{ message }}</p>
        }

        <fieldset class="format-list">
          <legend class="field-legend">Format</legend>
          @for (option of formats; track option.format) {
            <label class="format-option" [class.selected]="format() === option.format">
              <input
                type="radio"
                name="export-format"
                [value]="option.format"
                [checked]="format() === option.format"
                (change)="format.set(option.format)"
              />
              <span class="format-text">
                <span class="format-label">{{ option.label }}</span>
                <span class="format-description">{{ option.description }}</span>
              </span>
            </label>
          }
        </fieldset>

        @if (supportsTemplate()) {
          <div class="template-field">
            <app-select
              label="Template"
              helperText="Only affects this download — your resume keeps its own template."
              [options]="templateOptions()"
              [value]="template()"
              (valueChange)="template.set(+$event)"
            />
          </div>
        } @else {
          <!-- JSON carries no presentation, so offering a template would be misleading. -->
          <p class="json-note">
            JSON is a lossless copy of your data. It carries no styling, so no template
            applies — re-import it anywhere to get your resume back exactly as it is.
          </p>
        }

        <div class="dialog-actions">
          <app-button variant="ghost" (btnClick)="close()">Cancel</app-button>
          <app-button variant="primary" [loading]="downloading()" (btnClick)="download()">
            {{ downloading() ? 'Preparing…' : 'Download' }}
          </app-button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './export-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExportDialogComponent {
  private readonly resumes = inject(ResumeService);
  private readonly notifications = inject(NotificationService);

  readonly resume = input.required<Resume>();
  readonly templates = input.required<readonly ResumeTemplateDescriptor[]>();

  readonly closed = output<void>();

  protected readonly formats = EXPORT_FORMATS;
  protected readonly format = signal<ResumeFormat>(ResumeFormat.Pdf);
  protected readonly template = signal<ResumeTemplateKey>(ResumeTemplateKey.AtsFriendly);
  protected readonly downloading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly supportsTemplate = computed(() => this.format() !== ResumeFormat.Json);

  protected readonly templateOptions = computed<SelectOption[]>(() =>
    this.templates().map((descriptor) => ({
      value: descriptor.key,
      label: descriptor.isAtsSafe ? descriptor.name : `${descriptor.name} (not ATS-safe)`,
    })),
  );

  constructor() {
    // Defaults to the resume's own template so the common case is one click.
    queueMicrotask(() => this.template.set(this.resume().template));
  }

  protected close(): void {
    this.closed.emit();
  }

  protected download(): void {
    if (this.downloading()) return;

    this.downloading.set(true);
    this.errorMessage.set(null);

    const templateOverride = this.supportsTemplate() ? this.template() : undefined;

    this.resumes.export(this.resume().id, this.format(), templateOverride).subscribe({
      next: ({ blob, fileName }) => {
        this.downloading.set(false);
        saveBlob(blob, fileName);
        this.notifications.success('Resume exported', fileName);
        this.close();
      },
      error: (error: unknown) => {
        this.downloading.set(false);
        // Export responses are blobs, so an error body needs reading out of the blob
        // before it can be shown.
        void readBlobError(error).then((message) => this.errorMessage.set(message));
      },
    });
  }
}

/**
 * Triggers a download from an in-memory blob.
 *
 * The object URL is revoked immediately after the click; leaving it alive pins the
 * blob in memory for the lifetime of the document, which for repeated exports of a
 * multi-page PDF adds up.
 */
function saveBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');

  anchor.href = url;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);

  URL.revokeObjectURL(url);
}
