import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BadgeVariant = 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info';
export type BadgeStyle = 'solid' | 'soft' | 'outline';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [class]="badgeClasses()">
      <ng-content />
    </span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      gap: var(--space-1);
      padding: var(--space-1) var(--space-2);
      font-size: var(--text-caption);
      font-weight: 600;
      border-radius: var(--radius-pill);
      line-height: 1;
      white-space: nowrap;

      // Soft styles
      &-soft-primary { background-color: var(--brand-primary-alpha); color: var(--brand-primary); }
      &-soft-secondary { background-color: var(--bg-tertiary); color: var(--text-secondary); }
      &-soft-success { background-color: var(--success-bg); color: var(--success-text); }
      &-soft-warning { background-color: var(--warning-bg); color: var(--warning-text); }
      &-soft-danger { background-color: var(--danger-bg); color: var(--danger-text); }
      &-soft-info { background-color: var(--info-bg); color: var(--info-text); }

      // Solid styles
      &-solid-primary { background-color: var(--brand-primary); color: #ffffff; }
      &-solid-secondary { background-color: var(--text-secondary); color: #ffffff; }
      &-solid-success { background-color: var(--success); color: #ffffff; }
      &-solid-warning { background-color: var(--warning); color: #ffffff; }
      &-solid-danger { background-color: var(--danger); color: #ffffff; }
      &-solid-info { background-color: var(--info); color: #ffffff; }

      // Outline styles
      &-outline-primary { border: 1px solid var(--brand-primary); color: var(--brand-primary); }
      &-outline-secondary { border: 1px solid var(--border-color); color: var(--text-secondary); }
      &-outline-success { border: 1px solid var(--success); color: var(--success); }
      &-outline-warning { border: 1px solid var(--warning); color: var(--warning); }
      &-outline-danger { border: 1px solid var(--danger); color: var(--danger); }
      &-outline-info { border: 1px solid var(--info); color: var(--info); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BadgeComponent {
  readonly variant = input<BadgeVariant>('primary');
  readonly styleMode = input<BadgeStyle>('soft');

  protected badgeClasses = computed(() => {
    return `badge badge-${this.styleMode()}-${this.variant()}`;
  });
}
