import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ResumeService } from '../../../core/services/resume.service';
import { ResumeTemplateKey, ExportResumeOptions } from '../../../core/models/resume.models';
import {
  ButtonComponent,
  IconButtonComponent,
  IconComponent,
  SelectComponent,
  SelectOption,
  BadgeComponent,
} from '../../../shared/components';
import { ExportDialogComponent } from '../export-dialog/export-dialog.component';

@Component({
  selector: 'app-resume-preview',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    IconButtonComponent,
    IconComponent,
    SelectComponent,
    BadgeComponent,
    ExportDialogComponent,
  ],
  template: `
    <div class="preview-engine-layout">
      <!-- Top Control Bar -->
      <div class="preview-toolbar">
        <div class="toolbar-left">
          <app-select
            label="Template"
            [options]="templateOptions"
            [value]="selectedTemplate()"
            (valueChange)="onTemplateSwitch(+$event)"
          />
          @if (currentTemplateDescriptor()?.isAtsSafe) {
            <app-badge variant="success" styleMode="soft" class="ats-badge">
              <app-icon name="check-circle" size="xs" /> ATS Safe
            </app-badge>
          }
        </div>

        <div class="toolbar-center">
          <div class="zoom-controls">
            <app-icon-button icon="plus" size="sm" ariaLabel="Zoom in" (btnClick)="zoomIn()" />
            <span class="zoom-text">{{ zoomLevel() }}%</span>
            <app-icon-button icon="x" size="sm" ariaLabel="Reset zoom" (btnClick)="resetZoom()" />
          </div>

          <div class="viewport-toggle">
            <app-icon-button
              icon="monitor"
              size="sm"
              [variant]="viewport() === 'desktop' ? 'primary' : 'ghost'"
              ariaLabel="Desktop view"
              (btnClick)="viewport.set('desktop')"
            />
            <app-icon-button
              icon="grid"
              size="sm"
              [variant]="viewport() === 'tablet' ? 'primary' : 'ghost'"
              ariaLabel="Tablet view"
              (btnClick)="viewport.set('tablet')"
            />
          </div>
        </div>

        <div class="toolbar-right">
          <app-button variant="outline" (btnClick)="printPreview()">
            <app-icon name="calendar" size="sm" /> Print Preview
          </app-button>
          <app-button variant="primary" (btnClick)="isExportDialogOpen.set(true)">
            <app-icon name="download" size="sm" /> Export Document
          </app-button>
        </div>
      </div>

      <!-- Preview Canvas -->
      <div class="preview-viewport-container" [class]="'viewport-' + viewport()">
        <div
          class="preview-paper-frame"
          [style.transform]="'scale(' + (zoomLevel() / 100) + ')'"
        >
          <div class="live-preview-content" [innerHTML]="safeHtml()"></div>
        </div>
      </div>
    </div>

    <app-export-dialog
      [isOpen]="isExportDialogOpen()"
      [currentTemplate]="selectedTemplate()"
      (closed)="isExportDialogOpen.set(false)"
      (exportSubmit)="onExportSubmit($event)"
    />
  `,
  styles: [`
    .preview-engine-layout {
      display: flex;
      flex-direction: column;
      height: calc(100vh - 64px);
      background-color: var(--bg-tertiary);
    }
    .preview-toolbar {
      height: 56px;
      padding: 0 var(--space-6);
      background-color: var(--bg-elevated);
      border-bottom: 1px solid var(--border-color);
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      z-index: var(--z-sticky);
    }
    .toolbar-left, .toolbar-center, .toolbar-right {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }
    .ats-badge { margin-top: 18px; }
    .zoom-controls, .viewport-toggle {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      background-color: var(--bg-tertiary);
      padding: 2px 6px;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
    }
    .zoom-text {
      font-size: var(--text-caption);
      font-family: var(--font-mono);
      font-weight: 600;
      min-width: 42px;
      text-align: center;
    }
    .preview-viewport-container {
      flex: 1;
      overflow: auto;
      padding: var(--space-8);
      display: flex;
      justify-content: center;
      align-items: flex-start;

      &.viewport-tablet { max-width: 768px; margin: 0 auto; }
      &.viewport-mobile { max-width: 480px; margin: 0 auto; }
    }
    .preview-paper-frame {
      width: 800px;
      min-height: 1050px;
      background-color: #ffffff;
      color: #0f172a;
      box-shadow: var(--shadow-2xl);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      transform-origin: top center;
      transition: transform var(--duration-fast) var(--ease-fluent);
      overflow: hidden;
    }
    .live-preview-content {
      width: 100%;
      height: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResumePreviewComponent implements OnInit {
  private readonly resumeService = inject(ResumeService);
  private readonly sanitizer = inject(DomSanitizer);

  protected selectedTemplate = signal<ResumeTemplateKey>(ResumeTemplateKey.AtsFriendly);
  protected zoomLevel = signal<number>(100);
  protected viewport = signal<'desktop' | 'tablet' | 'mobile'>('desktop');
  protected isExportDialogOpen = signal<boolean>(false);
  protected rawHtml = signal<string>('');

  protected readonly safeHtml = computed<SafeHtml>(() =>
    this.sanitizer.bypassSecurityTrustHtml(this.rawHtml() || this.getDefaultPreviewHtml())
  );

  protected readonly templateOptions: SelectOption[] = [
    { label: 'ATS Friendly', value: ResumeTemplateKey.AtsFriendly },
    { label: 'Professional', value: ResumeTemplateKey.Professional },
    { label: 'Executive', value: ResumeTemplateKey.Executive },
    { label: 'Minimal', value: ResumeTemplateKey.Minimal },
    { label: 'Modern (2-Column)', value: ResumeTemplateKey.Modern },
    { label: 'Corporate', value: ResumeTemplateKey.Corporate },
  ];

  protected currentTemplateDescriptor = computed(() =>
    this.resumeService.templates().find((t) => t.key === this.selectedTemplate())
  );

  ngOnInit(): void {
    this.resumeService.loadTemplates().subscribe();
    this.updatePreviewHtml();
  }

  protected onTemplateSwitch(key: ResumeTemplateKey): void {
    this.selectedTemplate.set(key);
    this.updatePreviewHtml();
  }

  protected zoomIn(): void {
    if (this.zoomLevel() < 200) this.zoomLevel.update((z) => z + 15);
  }

  protected resetZoom(): void {
    this.zoomLevel.set(100);
  }

  protected printPreview(): void {
    window.print();
  }

  protected onExportSubmit(options: ExportResumeOptions): void {
    // Export handler stub
  }

  private updatePreviewHtml(): void {
    this.rawHtml.set(this.getDefaultPreviewHtml());
  }

  private getDefaultPreviewHtml(): string {
    const key = this.selectedTemplate();
    const accent = key === ResumeTemplateKey.Modern ? '#2563eb' : (key === ResumeTemplateKey.Professional ? '#1f3a5f' : '#1e293b');
    return `
      <div style="padding: 40px; font-family: sans-serif; color: #1e293b;">
        <div style="text-align: center; border-bottom: 2px solid ${accent}; padding-bottom: 16px; margin-bottom: 24px;">
          <h1 style="margin: 0; font-size: 26px; color: ${accent};">Alexander Wright</h1>
          <p style="margin: 4px 0 0 0; color: #64748b; font-size: 14px;">Principal Software Architect & Lead Engineer</p>
          <p style="margin: 6px 0 0 0; color: #475569; font-size: 12px;">alex.wright@example.com • +1 (555) 019-2834 • San Francisco, CA • linkedin.com/in/alexwright</p>
        </div>

        <div style="margin-bottom: 20px;">
          <h2 style="font-size: 14px; text-transform: uppercase; color: ${accent}; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px;">Professional Summary</h2>
          <p style="font-size: 13px; line-height: 1.6; color: #334155; margin-top: 8px;">
            Senior Principal Architect with 12+ years of experience designing scalable distributed applications, microservices, and modern web application design systems. Proven track record leading high-performing engineering teams.
          </p>
        </div>

        <div style="margin-bottom: 20px;">
          <h2 style="font-size: 14px; text-transform: uppercase; color: ${accent}; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px;">Work Experience</h2>
          
          <div style="margin-top: 12px;">
            <div style="display: flex; justify-content: space-between; font-weight: 600; font-size: 14px;">
              <span>Principal Software Architect</span>
              <span>2022 – Present</span>
            </div>
            <div style="color: #64748b; font-size: 12px; margin-bottom: 6px;">CareerPilot AI Tech Corp — San Francisco, CA</div>
            <ul style="margin: 0 0 0 18px; font-size: 13px; color: #334155;">
              <li>Architected enterprise CQRS & Signal-based web UI engine supporting lossless multi-template rendering.</li>
              <li>Engineered WCAG 2.2 AA compliant design system adopted across 4 core product lines.</li>
            </ul>
          </div>
        </div>

        <div>
          <h2 style="font-size: 14px; text-transform: uppercase; color: ${accent}; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px;">Skills & Competencies</h2>
          <div style="margin-top: 8px; font-size: 13px; color: #334155;">
            <strong>Technical:</strong> Angular 22, C# .NET 9, Clean Architecture, CQRS, Signals, SCSS Design Tokens, Microservices
          </div>
        </div>
      </div>
    `;
  }
}
