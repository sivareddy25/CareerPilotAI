import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  ResumeDocument,
  ResumeSectionKind,
  ResumeTemplateDescriptor,
  ResumeTemplateKey,
} from '../../../core/resumes/resume.models';

/**
 * The rendering engine. One component renders all five templates.
 *
 * This is the structural answer to "templates must not duplicate resume data". There is
 * exactly one component that knows how to draw a resume, and it draws the one
 * `ResumeDocument` it is given. A template contributes only three things:
 *
 * 1. **Section order**, taken from `descriptor.sectionOrder`.
 * 2. **Design tokens** — fonts, accent colour, base size — applied as CSS custom
 *    properties on the host.
 * 3. **A `data-template` attribute**, which structural CSS keys off for things tokens
 *    cannot express, such as the Modern template's two-column grid.
 *
 * Five separate template components would each have had to restate the whole document
 * structure, and the fifth would have quietly diverged the first time a field was added.
 * Here a change to how experience renders lands in every template at once, and adding a
 * sixth template is a catalogue entry plus a CSS block — no TypeScript at all.
 *
 * The same descriptors drive the server's PDF and DOCX exporters, so what is on screen
 * and what downloads come from one definition.
 */
@Component({
  selector: 'app-resume-renderer',
  standalone: true,
  templateUrl: './resume-renderer.component.html',
  styleUrl: './resume-renderer.component.scss',
  host: {
    '[attr.data-template]': 'templateName()',
    '[style.--resume-accent]': 'descriptor().accentColor',
    '[style.--resume-heading-font]': 'quoted(descriptor().headingFont)',
    '[style.--resume-body-font]': 'quoted(descriptor().bodyFont)',
    '[style.--resume-base-size.pt]': 'descriptor().baseFontSize',
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResumeRendererComponent {
  readonly document = input.required<ResumeDocument>();
  readonly descriptor = input.required<ResumeTemplateDescriptor>();

  /** Exposed so the template can enumerate sections without importing the enum. */
  protected readonly Section = ResumeSectionKind;

  protected readonly templateName = computed(() => ResumeTemplateKey[this.descriptor().key]);

  /**
   * Sections in the template's order, with empty ones dropped.
   *
   * Filtering here rather than in the template keeps the markup free of a condition per
   * section, and means an empty resume renders as a clean page rather than a column of
   * bare headings.
   */
  protected readonly sections = computed(() => {
    const doc = this.document();

    return this.descriptor().sectionOrder.filter((section) => {
      switch (section) {
        case ResumeSectionKind.Summary:
          return !!doc.summary?.trim();
        case ResumeSectionKind.Experience:
          return doc.experience.length > 0;
        case ResumeSectionKind.Education:
          return doc.education.length > 0;
        case ResumeSectionKind.Skills:
          return doc.skills.length > 0;
        case ResumeSectionKind.Projects:
          return doc.projects.length > 0;
        case ResumeSectionKind.Certifications:
          return doc.certifications.length > 0;
        default:
          return false;
      }
    });
  });

  /**
   * Skills grouped by category.
   *
   * The grouping is derived, never stored twice — the document holds a flat list, and
   * templates that do not group simply render the ungrouped bucket.
   */
  protected readonly skillGroups = computed(() => {
    const groups = new Map<string, string[]>();

    for (const skill of this.document().skills) {
      const key = skill.category?.trim() || '';
      const existing = groups.get(key);

      if (existing) {
        existing.push(skill.name);
      } else {
        groups.set(key, [skill.name]);
      }
    }

    return [...groups.entries()].map(([category, names]) => ({ category, names }));
  });

  /** Contact details as one line, matching how the PDF and DOCX exporters lay them out. */
  protected readonly contactParts = computed(() => {
    const contact = this.document().contact;

    return [
      contact.email,
      contact.phone,
      contact.location,
      contact.linkedIn,
      contact.gitHub,
      contact.website,
    ].filter((part): part is string => !!part?.trim());
  });

  protected dateRange(start: string | null, end: string | null, isCurrent = false): string {
    const to = isCurrent ? 'Present' : end;

    if (!start) return to ?? '';

    return to ? `${start} – ${to}` : start;
  }

  protected joinDefined(separator: string, ...parts: (string | null | undefined)[]): string {
    return parts.filter((part) => !!part?.trim()).join(separator);
  }

  /** Font family names with spaces need quoting before they reach a CSS custom property. */
  protected quoted(font: string): string {
    return `"${font}", sans-serif`;
  }
}
