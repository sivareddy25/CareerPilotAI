import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

/**
 * Shows an AI match score as a coloured "NN% Match" pill.
 *
 * Renders nothing when the score is null — the state when the user has no scorable career
 * profile — so callers can drop it into any card without first checking. The colour bands are
 * intentionally coarse (strong / good / partial / weak) so the pill communicates fit at a glance
 * rather than implying false precision from a single number.
 */
@Component({
  selector: 'app-match-score',
  standalone: true,
  template: `
    @if (score() !== null && score() !== undefined) {
      <span class="match-pill" [class]="'band-' + band()" [title]="summary() || (score() + '% match')">
        <span class="match-value">{{ score() }}%</span>
        @if (showLabel()) {
          <span class="match-label">Match</span>
        }
      </span>
    }
  `,
  styles: [`
    .match-pill {
      display: inline-flex;
      align-items: baseline;
      gap: 4px;
      padding: 2px 8px;
      border-radius: var(--radius-full, 999px);
      font-weight: 700;
      font-size: var(--text-caption, 0.75rem);
      line-height: 1.4;
      white-space: nowrap;
    }
    .match-value { font-variant-numeric: tabular-nums; }
    .match-label { font-weight: 600; opacity: 0.8; }

    .band-strong  { background: color-mix(in srgb, var(--color-success, #16a34a) 18%, transparent); color: var(--color-success, #16a34a); }
    .band-good    { background: color-mix(in srgb, var(--brand-primary, #2563eb) 16%, transparent); color: var(--brand-primary, #2563eb); }
    .band-partial { background: color-mix(in srgb, var(--color-warning, #d97706) 18%, transparent); color: var(--color-warning, #d97706); }
    .band-weak    { background: var(--surface-muted, #e5e7eb); color: var(--text-muted, #6b7280); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatchScoreComponent {
  readonly score = input.required<number | null | undefined>();
  readonly summary = input<string | null | undefined>(null);
  readonly showLabel = input<boolean>(true);

  protected readonly band = computed(() => {
    const value = this.score() ?? 0;
    if (value >= 85) return 'strong';
    if (value >= 70) return 'good';
    if (value >= 50) return 'partial';
    return 'weak';
  });
}
