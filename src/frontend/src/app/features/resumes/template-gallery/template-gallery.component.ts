import { Component, ChangeDetectionStrategy, inject, OnInit, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResumeService } from '../../../core/services/resume.service';
import { ResumeTemplateDescriptor, ResumeTemplateKey } from '../../../core/models/resume.models';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  IconComponent,
  PageHeaderComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-template-gallery',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent,
    PageHeaderComponent,
  ],
  template: `
    <div class="gallery-container">
      <app-page-header
        title="Resume Template Gallery"
        subtitle="Choose from 6 production-grade template styles. All templates share identical domain data."
      />

      <div class="gallery-grid">
        @for (tpl of templateList(); track tpl.key) {
          <app-card
            [title]="tpl.name"
            [subtitle]="tpl.columns === 1 ? 'Single Column' : 'Two Column Layout'"
            [hoverable]="true"
            [class.selected-card]="selectedKey() === tpl.key"
          >
            <div class="template-card-preview">
              <div class="mini-document-wireframe" [style.--accent]="tpl.accentColor">
                <div class="mini-header"></div>
                <div class="mini-line long"></div>
                <div class="mini-line medium"></div>
                <div class="mini-line short"></div>
              </div>
            </div>

            <p class="template-desc">{{ tpl.description }}</p>

            <div class="template-tags">
              @if (tpl.isAtsSafe) {
                <app-badge variant="success" styleMode="soft">
                  <app-icon name="check-circle" size="xs" /> ATS Safe
                </app-badge>
              } @else {
                <app-badge variant="warning" styleMode="soft">
                  <app-icon name="alert-triangle" size="xs" /> Visual / 2-Column
                </app-badge>
              }
              <app-badge variant="secondary" styleMode="outline">
                {{ tpl.bodyFont }}
              </app-badge>
            </div>

            <div card-footer class="template-actions">
              <app-button
                [variant]="selectedKey() === tpl.key ? 'primary' : 'outline'"
                [fullWidth]="true"
                (btnClick)="selectTemplate(tpl.key)"
              >
                {{ selectedKey() === tpl.key ? 'Active Template' : 'Use Template' }}
              </app-button>
            </div>
          </app-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .gallery-container {
      padding: var(--space-6);
      max-width: 1280px;
      margin: 0 auto;
    }
    .gallery-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: var(--space-6);
      margin-top: var(--space-6);
    }
    .selected-card {
      border-color: var(--brand-primary) !important;
      box-shadow: var(--shadow-focus) !important;
    }
    .template-card-preview {
      height: 140px;
      background-color: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      margin-bottom: var(--space-4);
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
    }
    .mini-document-wireframe {
      width: 100px;
      height: 120px;
      background-color: var(--bg-elevated);
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-color);
      padding: var(--space-2);
      box-shadow: var(--shadow-xs);
    }
    .mini-header {
      height: 8px;
      background-color: var(--accent);
      border-radius: 2px;
      margin-bottom: var(--space-2);
    }
    .mini-line {
      height: 4px;
      background-color: var(--border-strong);
      border-radius: 2px;
      margin-bottom: 4px;
      &.long { width: 100%; }
      &.medium { width: 75%; }
      &.short { width: 50%; }
    }
    .template-desc {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      min-height: 40px;
      margin: 0 0 var(--space-4) 0;
    }
    .template-tags {
      display: flex;
      gap: var(--space-2);
      margin-bottom: var(--space-4);
    }
    .template-actions {
      padding: var(--space-3);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TemplateGalleryComponent implements OnInit {
  private readonly resumeService = inject(ResumeService);

  protected readonly templateList = this.resumeService.templates;
  protected selectedKey = signal<ResumeTemplateKey>(ResumeTemplateKey.AtsFriendly);

  readonly templateSelect = output<ResumeTemplateKey>();

  ngOnInit(): void {
    this.resumeService.loadTemplates().subscribe();
  }

  protected selectTemplate(key: ResumeTemplateKey): void {
    this.selectedKey.set(key);
    this.templateSelect.emit(key);
  }
}
