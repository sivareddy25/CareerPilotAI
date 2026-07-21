import { Component, ChangeDetectionStrategy, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="file-upload-wrapper">
      @if (label()) {
        <label class="field-label">{{ label() }}</label>
      }
      <div class="drop-zone" [class.dragover]="isDragging()" (click)="triggerFileSelect()">
        <div class="upload-icon">
          <app-icon name="upload" size="lg" />
        </div>
        <p class="drop-title">Drag & drop your files here, or <span class="browse-link">browse</span></p>
        <p class="drop-subtitle">{{ accept() }} (Max {{ maxFileSize() }})</p>
      </div>

      @if (files().length > 0) {
        <div class="file-list">
          @for (file of files(); track file.name) {
            <div class="file-item">
              <app-icon name="file-text" size="sm" />
              <div class="file-info">
                <span class="file-name">{{ file.name }}</span>
                <span class="file-size">{{ file.size }}</span>
              </div>
              <button type="button" class="remove-btn" (click)="removeFile(file.name)">
                <app-icon name="x" size="xs" />
              </button>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .file-upload-wrapper {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      width: 100%;
    }
    .field-label {
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-primary);
    }
    .drop-zone {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--space-6) var(--space-4);
      border: 2px dashed var(--border-color);
      border-radius: var(--radius-xl);
      background-color: var(--bg-secondary);
      cursor: pointer;
      text-align: center;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover, &.dragover {
        border-color: var(--brand-primary);
        background-color: var(--brand-primary-alpha);
      }
    }
    .upload-icon {
      width: 48px;
      height: 48px;
      border-radius: var(--radius-circle);
      background-color: var(--bg-tertiary);
      color: var(--brand-primary);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: var(--space-2);
    }
    .drop-title {
      margin: 0 0 var(--space-1) 0;
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-primary);
    }
    .browse-link { color: var(--brand-primary); font-weight: 600; text-decoration: underline; }
    .drop-subtitle {
      margin: 0;
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
    .file-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      margin-top: var(--space-2);
    }
    .file-item {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2) var(--space-3);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
    }
    .file-info {
      display: flex;
      flex-direction: column;
      flex: 1;
    }
    .file-name { font-size: var(--text-body-sm); font-weight: 500; }
    .file-size { font-size: var(--text-caption); color: var(--text-muted); }
    .remove-btn {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      &:hover { color: var(--danger); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FileUploadComponent {
  readonly label = input<string>();
  readonly accept = input<string>('PDF, DOCX, PNG or JPG');
  readonly maxFileSize = input<string>('10 MB');

  protected isDragging = signal<boolean>(false);
  protected files = signal<{ name: string; size: string }[]>([
    { name: 'sample_resume_2026.pdf', size: '1.2 MB' }
  ]);

  protected triggerFileSelect(): void {
    // Placeholder interaction
  }

  protected removeFile(name: string): void {
    this.files.update((list) => list.filter((f) => f.name !== name));
  }
}
