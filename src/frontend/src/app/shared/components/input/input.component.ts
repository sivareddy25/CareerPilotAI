import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';

@Component({
  selector: 'app-input',
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
      <div class="input-container">
        @if (leadingIcon()) {
          <app-icon [name]="leadingIcon()!" size="sm" class="leading-icon" />
        }
        <input
          [id]="id()"
          [type]="type()"
          [value]="value()"
          [placeholder]="placeholder()"
          [disabled]="disabled()"
          [readonly]="readonly()"
          [attr.aria-invalid]="!!error()"
          [attr.aria-describedby]="error() ? id() + '-error' : (helperText() ? id() + '-helper' : null)"
          class="input-control"
          [class.has-leading]="!!leadingIcon()"
          [class.has-trailing]="!!trailingIcon()"
          (input)="onInputChange($event)"
        />
        @if (trailingIcon()) {
          <app-icon [name]="trailingIcon()!" size="sm" class="trailing-icon" />
        }
      </div>
      @if (error()) {
        <p [id]="id() + '-error'" class="field-error">{{ error() }}</p>
      } @else if (helperText()) {
        <p [id]="id() + '-helper'" class="field-helper">{{ helperText() }}</p>
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
    .input-container {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
    }
    .input-control {
      width: 100%;
      height: 40px;
      padding: var(--space-2) var(--space-3);
      font-size: var(--text-body-sm);
      font-family: var(--font-sans);
      color: var(--text-primary);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      transition: all var(--duration-fast) var(--ease-fluent);
      box-sizing: border-box;

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

      &.has-leading { padding-left: 36px; }
      &.has-trailing { padding-right: 36px; }
    }
    .leading-icon, .trailing-icon {
      position: absolute;
      color: var(--text-muted);
      pointer-events: none;
    }
    .leading-icon { left: 10px; }
    .trailing-icon { right: 10px; }

    .field-wrapper.has-error {
      .input-control {
        border-color: var(--danger);
        &:focus { box-shadow: 0 0 0 3px var(--danger-bg); }
      }
    }
    .field-error {
      margin: 0;
      font-size: var(--text-caption);
      color: var(--danger);
    }
    .field-helper {
      margin: 0;
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InputComponent {
  readonly id = input<string>(`input-${Math.random().toString(36).substr(2, 9)}`);
  readonly label = input<string>();
  readonly type = input<string>('text');
  readonly value = input<string>('');
  readonly placeholder = input<string>('');
  readonly helperText = input<string>();
  readonly error = input<string>();
  readonly leadingIcon = input<IconName>();
  readonly trailingIcon = input<IconName>();
  readonly disabled = input<boolean>(false);
  readonly readonly = input<boolean>(false);
  readonly required = input<boolean>(false);

  readonly valueChange = output<string>();

  protected onInputChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.valueChange.emit(val);
  }
}
