import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface RadioOption {
  label: string;
  value: string | number;
  disabled?: boolean;
}

@Component({
  selector: 'app-radio',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="radio-group" [class.horizontal]="layout() === 'horizontal'">
      @for (option of options(); track option.value) {
        <label class="radio-wrapper" [class.disabled]="option.disabled || disabled()">
          <input
            type="radio"
            [name]="name()"
            [value]="option.value"
            [checked]="value() === option.value"
            [disabled]="option.disabled || disabled()"
            class="sr-only"
            (change)="onSelect(option.value)"
          />
          <span class="radio-circle" [class.checked]="value() === option.value">
            <span class="radio-dot"></span>
          </span>
          <span class="radio-label">{{ option.label }}</span>
        </label>
      }
    </div>
  `,
  styles: [`
    .radio-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);

      &.horizontal {
        flex-direction: row;
        flex-wrap: wrap;
        gap: var(--space-4);
      }
    }
    .radio-wrapper {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      cursor: pointer;
      user-select: none;
      font-size: var(--text-body-sm);
      color: var(--text-primary);

      &.disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }
    .radio-circle {
      width: 18px;
      height: 18px;
      border-radius: var(--radius-circle);
      border: 1px solid var(--border-strong);
      background-color: var(--bg-elevated);
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--duration-fast) var(--ease-fluent);

      &.checked {
        border-color: var(--brand-primary);
        .radio-dot { transform: scale(1); }
      }
    }
    .radio-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-circle);
      background-color: var(--brand-primary);
      transform: scale(0);
      transition: transform var(--duration-fast) var(--ease-fluent);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RadioComponent {
  readonly name = input<string>(`radio-group-${Math.random().toString(36).substr(2, 9)}`);
  readonly options = input.required<RadioOption[]>();
  readonly value = input<string | number>('');
  readonly layout = input<'vertical' | 'horizontal'>('vertical');
  readonly disabled = input<boolean>(false);

  readonly valueChange = output<string | number>();

  protected onSelect(val: string | number): void {
    if (!this.disabled()) {
      this.valueChange.emit(val);
    }
  }
}
