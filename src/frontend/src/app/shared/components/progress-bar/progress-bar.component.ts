import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ProgressVariant = 'primary' | 'success' | 'warning' | 'danger';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="progress-container"
      role="progressbar"
      [attr.aria-valuenow]="indeterminate() ? null : value()"
      [attr.aria-valuemin]="0"
      [attr.aria-valuemax]="100"
      [attr.aria-label]="label() || 'Progress'"
    >
      <div [class]="barClasses()" [style.width.%]="indeterminate() ? null : value()"></div>
    </div>
  `,
  styles: [`
    .progress-container {
      width: 100%;
      height: 8px;
      background-color: var(--bg-tertiary);
      border-radius: var(--radius-pill);
      overflow: hidden;
      position: relative;
    }
    .progress-bar {
      height: 100%;
      border-radius: var(--radius-pill);
      transition: width var(--duration-normal) var(--ease-fluent);

      &-primary { background-color: var(--brand-primary); }
      &-success { background-color: var(--success); }
      &-warning { background-color: var(--warning); }
      &-danger { background-color: var(--danger); }

      &.indeterminate {
        width: 40% !important;
        position: absolute;
        animation: progressIndeterminate 1.5s infinite var(--ease-fluent);
      }
    }

    @keyframes progressIndeterminate {
      0% { left: -40%; }
      100% { left: 100%; }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProgressBarComponent {
  readonly value = input<number>(0);
  readonly indeterminate = input<boolean>(false);
  readonly variant = input<ProgressVariant>('primary');
  readonly label = input<string>();

  protected barClasses = computed(() => {
    return [
      'progress-bar',
      `progress-bar-${this.variant()}`,
      this.indeterminate() ? 'indeterminate' : '',
    ].filter(Boolean).join(' ');
  });
}
