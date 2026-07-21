import { Component, ChangeDetectionStrategy, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  DialogComponent,
  ButtonComponent,
  RadioComponent,
  RadioOption,
  SelectComponent,
  SelectOption,
  ToggleComponent,
  IconComponent,
} from '../../../shared/components';
import {
  ResumeFormat,
  ResumeTemplateKey,
  ResumePageSize,
  ResumeMarginSize,
  ExportResumeOptions,
} from '../../../core/models/resume.models';

@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DialogComponent,
    ButtonComponent,
    RadioComponent,
    SelectComponent,
    ToggleComponent,
    IconComponent,
  ],
  template: `
    <app-dialog
      [isOpen]="isOpen()"
      title="Export Resume Document"
      size="md"
      (closed)="close()"
    >
      <div class="export-form">
        <h4 class="form-section-title">Select Format</h4>
        <app-radio
          name="export-format"
          [options]="formatOptions"
          [value]="selectedFormat()"
          layout="horizontal"
          (valueChange)="onFormatChange($event)"
        />

        <h4 class="form-section-title">Page & Layout Settings</h4>
        <div class="form-grid">
          <app-select
            label="Page Size"
            [options]="pageSizeOptions"
            [value]="selectedPageSize()"
            (valueChange)="selectedPageSize.set(+$event)"
          />
          <app-select
            label="Page Margins"
            [options]="marginOptions"
            [value]="selectedMargin()"
            (valueChange)="selectedMargin.set(+$event)"
          />
        </div>

        <div class="toggle-group">
          <app-toggle
            label="Include Page Numbers"
            [checked]="includePageNumbers()"
            (checkedChange)="includePageNumbers.set($event)"
          />
          <app-toggle
            label="Include Header / Footer"
            [checked]="includeHeaderFooter()"
            (checkedChange)="includeHeaderFooter.set($event)"
          />
        </div>
      </div>

      <div dialog-footer class="export-actions">
        <app-button variant="secondary" (btnClick)="close()">Cancel</app-button>
        <app-button variant="primary" (btnClick)="submitExport()">
          <app-icon name="download" size="sm" /> Download File
        </app-button>
      </div>
    </app-dialog>
  `,
  styles: [`
    .export-form {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .form-section-title {
      font-size: var(--text-body-sm);
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
    }
    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }
    .toggle-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      padding-top: var(--space-2);
    }
    .export-actions {
      display: flex;
      gap: var(--space-3);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExportDialogComponent {
  readonly isOpen = input<boolean>(false);
  readonly currentTemplate = input<ResumeTemplateKey>(ResumeTemplateKey.AtsFriendly);

  readonly closed = output<void>();
  readonly exportSubmit = output<ExportResumeOptions>();

  protected selectedFormat = signal<ResumeFormat>(ResumeFormat.Pdf);
  protected selectedPageSize = signal<ResumePageSize>(ResumePageSize.A4);
  protected selectedMargin = signal<ResumeMarginSize>(ResumeMarginSize.Normal);
  protected includePageNumbers = signal<boolean>(true);
  protected includeHeaderFooter = signal<boolean>(false);

  protected readonly formatOptions: RadioOption[] = [
    { label: 'PDF Document (.pdf)', value: ResumeFormat.Pdf },
    { label: 'Word Document (.docx)', value: ResumeFormat.Docx },
    { label: 'JSON Data (.json)', value: ResumeFormat.Json },
  ];

  protected readonly pageSizeOptions: SelectOption[] = [
    { label: 'A4 (210 x 297 mm)', value: ResumePageSize.A4 },
    { label: 'US Letter (8.5 x 11 in)', value: ResumePageSize.Letter },
  ];

  protected readonly marginOptions: SelectOption[] = [
    { label: 'Normal (0.75 in)', value: ResumeMarginSize.Normal },
    { label: 'Narrow (0.5 in)', value: ResumeMarginSize.Narrow },
    { label: 'Wide (1.0 in)', value: ResumeMarginSize.Wide },
  ];

  protected onFormatChange(val: string | number): void {
    this.selectedFormat.set(+val as ResumeFormat);
  }

  protected close(): void {
    this.closed.emit();
  }

  protected submitExport(): void {
    this.exportSubmit.emit({
      format: this.selectedFormat(),
      template: this.currentTemplate(),
      pageSize: this.selectedPageSize(),
      margin: this.selectedMargin(),
      includePageNumbers: this.includePageNumbers(),
      includeHeaderFooter: this.includeHeaderFooter(),
    });
    this.close();
  }
}
