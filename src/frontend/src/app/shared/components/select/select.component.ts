import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

export interface SelectOption {
  label: string;
  value: string | number;
  disabled?: boolean;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="field-wrapper" [class.has-error]="!!error()">
      @if (label()) {
        <label [for]="id()" class="field-label">
          {{ label() }}
          @if (required()) { <span class="required-star">*</span> }
        </label>
      }
      <div class="select-container">
        <select
          [id]="id()"
          [value]="value()"
          [disabled]="disabled()"
          class="select-control"
          (change)="onSelectChange($event)"
        >
          @if (placeholder()) {
            <option value="" disabled [selected]="!value()">{{ placeholder() }}</option>
          }
          @for (option of options(); track option.value) {
            <option [value]="option.value" [disabled]="option.disabled">
              {{ option.label }}
            </option>
          }
        </select>
        <app-icon name="chevron-down" size="sm" class="select-arrow" />
      </div>
      @if (error()) {
        <p class="field-error">{{ error() }}</p>
      } @else if (helperText()) {
        <p class="field-helper">{{ helperText() }}</p>
      }
    </div>
  `,
  styles: [`
    .field-wrapper {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
      width: 100%;
    }
    .field-label {
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-primary);
      .required-star { color: var(--danger); }
    }
    .select-container {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
    }
    .select-control {
      width: 100%;
      height: 40px;
      padding: var(--space-2) 36px var(--space-2) var(--space-3);
      font-size: var(--text-body-sm);
      font-family: var(--font-sans);
      color: var(--text-primary);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      appearance: none;
      box-sizing: border-box;
      transition: all var(--duration-fast) var(--ease-fluent);
      cursor: pointer;

      &:focus {
        outline: none;
        border-color: var(--border-focus);
        box-shadow: var(--shadow-focus);
      }

      &:disabled {
        background-color: var(--bg-tertiary);
        color: var(--text-muted);
        cursor: not-allowed;
      }
    }
    .select-arrow {
      position: absolute;
      right: 10px;
      color: var(--text-muted);
      pointer-events: none;
    }
    .field-error { margin: 0; font-size: var(--text-caption); color: var(--danger); }
    .field-helper { margin: 0; font-size: var(--text-caption); color: var(--text-muted); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SelectComponent {
  readonly id = input<string>(`select-${Math.random().toString(36).substr(2, 9)}`);
  readonly label = input<string>();
  readonly value = input<string | number>('');
  readonly placeholder = input<string>('Select an option...');
  readonly options = input.required<SelectOption[]>();
  readonly helperText = input<string>();
  readonly error = input<string>();
  readonly disabled = input<boolean>(false);
  readonly required = input<boolean>(false);

  readonly valueChange = output<string | number>();

  protected onSelectChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.valueChange.emit(val);
  }
}
