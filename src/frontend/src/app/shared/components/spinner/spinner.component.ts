import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SpinnerSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="spinnerClasses()" role="status" aria-label="Loading">
      <span class="sr-only">Loading...</span>
    </div>
  `,
  styles: [`
    .spinner {
      display: inline-block;
      border: 3px solid var(--border-color);
      border-top-color: var(--brand-primary);
      border-radius: var(--radius-circle);
      animation: spin 0.75s linear infinite;

      &-sm { width: 18px; height: 18px; border-width: 2px; }
      &-md { width: 32px; height: 32px; border-width: 3px; }
      &-lg { width: 48px; height: 48px; border-width: 4px; }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpinnerComponent {
  readonly size = input<SpinnerSize>('md');

  protected spinnerClasses = computed(() => `spinner spinner-${this.size()}`);
}
