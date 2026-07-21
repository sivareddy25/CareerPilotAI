import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ResumeService } from '../../../core/services/resume.service';
import { ResumeImportResultDto } from '../../../core/models/resume.models';
import {
  CardComponent,
  ButtonComponent,
  FileUploadComponent,
  ProgressBarComponent,
  PageHeaderComponent,
  BadgeComponent,
  IconComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-import-wizard',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    FileUploadComponent,
    ProgressBarComponent,
    PageHeaderComponent,
    BadgeComponent,
    IconComponent,
  ],
  template: `
    <div class="import-wizard-container">
      <app-page-header
        title="Import Resume Document"
        subtitle="Upload DOCX, PDF, or internal JSON files. Extracted content is mapped to the canonical schema."
      />

      <app-card class="wizard-card">
        @if (isUploading()) {
          <div class="uploading-state">
            <h3 class="state-title">Parsing & Validating Document...</h3>
            <p class="state-desc">Detecting file signature and extracting text content.</p>
            <app-progress-bar [indeterminate]="true" variant="primary" />
          </div>
        } @else if (importResult()) {
          <div class="result-state">
            <div class="result-header">
              <app-icon name="check-circle" size="xl" class="success-icon" />
              <h3>Import Complete!</h3>
              <p>Successfully imported {{ importResult()?.importedCount }} document(s).</p>
            </div>

            <div class="result-items">
              @for (item of importResult()?.results; track item.fileName) {
                <div class="result-item" [class.failed]="!item.succeeded">
                  <div class="item-title-bar">
                    <app-icon [name]="item.succeeded ? 'file-text' : 'alert-triangle'" size="sm" />
                    <span class="file-name">{{ item.fileName }}</span>
                    <app-badge [variant]="item.succeeded ? 'success' : 'danger'">
                      {{ item.succeeded ? 'Success' : 'Failed' }}
                    </app-badge>
                  </div>
                  @if (item.warnings.length > 0) {
                    <ul class="warning-list">
                      @for (w of item.warnings; track w) {
                        <li>{{ w }}</li>
                      }
                    </ul>
                  }
                  @if (item.error) {
                    <p class="error-msg">{{ item.error }}</p>
                  }
                </div>
              }
            </div>

            <div class="wizard-footer">
              <app-button variant="outline" (btnClick)="reset()">Import Another</app-button>
              <app-button variant="primary" (btnClick)="goToGallery()">View Resume Gallery</app-button>
            </div>
          </div>
        } @else {
          <div class="drop-step">
            <app-file-upload
              label="Select Resume File(s)"
              accept="PDF, DOCX, or JSON format"
              maxFileSize="10 MB per file"
            />

            <div class="wizard-actions">
              <app-button variant="primary" size="lg" (btnClick)="simulateUpload()">
                <app-icon name="upload" size="sm" /> Upload & Parse Document
              </app-button>
            </div>
          </div>
        }
      </app-card>
    </div>
  `,
  styles: [`
    .import-wizard-container {
      padding: var(--space-6);
      max-width: 800px;
      margin: 0 auto;
    }
    .wizard-card {
      margin-top: var(--space-6);
    }
    .drop-step, .uploading-state, .result-state {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    .wizard-actions, .wizard-footer {
      display: flex;
      justify-content: flex-end;
      gap: var(--space-3);
    }
    .result-header {
      text-align: center;
      .success-icon { color: var(--success); }
      h3 { margin: var(--space-2) 0 var(--space-1) 0; font-size: var(--text-h3); }
      p { margin: 0; color: var(--text-secondary); font-size: var(--text-body-sm); }
    }
    .result-items {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    .result-item {
      padding: var(--space-3) var(--space-4);
      background-color: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);

      &.failed { border-color: var(--danger-border); background-color: var(--danger-bg); }
    }
    .item-title-bar {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      .file-name { flex: 1; font-weight: 600; font-size: var(--text-body-sm); }
    }
    .warning-list {
      margin: var(--space-2) 0 0 var(--space-6);
      padding: 0;
      font-size: var(--text-caption);
      color: var(--warning-text);
    }
    .error-msg {
      margin: var(--space-2) 0 0 0;
      font-size: var(--text-caption);
      color: var(--danger);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImportWizardComponent {
  private readonly resumeService = inject(ResumeService);
  private readonly router = inject(Router);

  protected isUploading = signal<boolean>(false);
  protected importResult = signal<ResumeImportResultDto | null>(null);

  protected simulateUpload(): void {
    this.isUploading.set(true);

    setTimeout(() => {
      this.isUploading.set(false);
      this.importResult.set({
        results: [
          {
            fileName: 'sample_resume_2026.pdf',
            succeeded: true,
            resumeId: '12345',
            title: 'Software Engineer Resume',
            warnings: ['Contact phone extracted without country code.'],
          },
        ],
        allSucceeded: true,
        importedCount: 1,
      });
    }, 1500);
  }

  protected reset(): void {
    this.importResult.set(null);
    this.isUploading.set(false);
  }

  protected goToGallery(): void {
    this.router.navigate(['/resumes/templates']);
  }
}
