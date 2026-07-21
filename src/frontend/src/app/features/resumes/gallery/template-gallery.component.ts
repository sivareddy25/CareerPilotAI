import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ResumeRendererComponent } from '../renderer/resume-renderer.component';
import {
  ResumeDocument,
  ResumeTemplateDescriptor,
  ResumeTemplateKey,
} from '../../../core/resumes/resume.models';

/**
 * Template picker showing each option rendered with the user's own resume.
 *
 * Every tile runs the same rendering engine as the full preview, scaled down. Static
 * thumbnail images would be cheaper, but they go stale the moment a template changes
 * and they show a stranger's resume rather than the user's — which is precisely the
 * information needed to choose.
 */
@Component({
  selector: 'app-template-gallery',
  standalone: true,
  imports: [BadgeComponent, ResumeRendererComponent],
  template: `
    <div class="gallery-grid" role="radiogroup" aria-label="Resume templates">
      @for (template of templates(); track template.key) {
        <div
          class="template-card"
          role="radio"
          [attr.aria-checked]="selected() === template.key"
          [class.selected]="selected() === template.key"
          tabindex="0"
          (click)="choose(template.key)"
          (keydown.enter)="choose(template.key)"
          (keydown.space)="$event.preventDefault(); choose(template.key)"
        >
          <div class="thumbnail" aria-hidden="true">
            <!--
              The live renderer at ~22% scale. inert so nothing inside the miniature is
              focusable or announced — the card itself is the control.
            -->
            <div class="thumbnail-inner" inert>
              <app-resume-renderer [document]="document()" [descriptor]="template" />
            </div>
          </div>

          <div class="template-meta">
            <div class="template-name-row">
              <span class="template-name">{{ template.name }}</span>
              <app-badge [variant]="template.isAtsSafe ? 'success' : 'warning'">
                {{ template.isAtsSafe ? 'ATS safe' : 'Not ATS safe' }}
              </app-badge>
            </div>
            <p class="template-description">{{ template.description }}</p>
          </div>
        </div>
      }
    </div>
  `,
  styleUrl: './template-gallery.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TemplateGalleryComponent {
  readonly templates = input.required<readonly ResumeTemplateDescriptor[]>();
  readonly document = input.required<ResumeDocument>();
  readonly selected = input<ResumeTemplateKey>();

  readonly templateChosen = output<ResumeTemplateKey>();

  protected choose(key: ResumeTemplateKey): void {
    this.templateChosen.emit(key);
  }
}
