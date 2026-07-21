import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-textarea',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="field-wrapper" [class.has-error]="!!error()">
      @if (label()) {
        <label [for]="id()" class="field-label">
          {{ label() }}
          @if (required()) { <span class="required-star">*</span> }
        </label>
      }
      <textarea
        [id]="id()"
        [value]="value()"
        [rows]="rows()"
        [placeholder]="placeholder()"
        [disabled]="disabled()"
        [readonly]="readonly()"
        [maxLength]="maxLength() || 99999"
        class="textarea-control"
        (input)="onInputChange($event)"
      ></textarea>

      <div class="textarea-footer">
        @if (error()) {
          <p class="field-error">{{ error() }}</p>
        } @else if (helperText()) {
          <p class="field-helper">{{ helperText() }}</p>
        }
        @if (maxLength()) {
          <span class="char-counter">{{ value().length }} / {{ maxLength() }}</span>
        }
      </div>
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
    .textarea-control {
      width: 100%;
      padding: var(--space-2) var(--space-3);
      font-size: var(--text-body-sm);
      font-family: var(--font-sans);
      color: var(--text-primary);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      resize: vertical;
      box-sizing: border-box;
      transition: all var(--duration-fast) var(--ease-fluent);

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
    .textarea-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: var(--text-caption);
    }
    .field-error { margin: 0; color: var(--danger); }
    .field-helper { margin: 0; color: var(--text-muted); }
    .char-counter { margin-left: auto; color: var(--text-muted); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TextareaComponent {
  readonly id = input<string>(`textarea-${Math.random().toString(36).substr(2, 9)}`);
  readonly label = input<string>();
  readonly value = input<string>('');
  readonly placeholder = input<string>('');
  readonly rows = input<number>(4);
  readonly helperText = input<string>();
  readonly error = input<string>();
  readonly maxLength = input<number>();
  readonly disabled = input<boolean>(false);
  readonly readonly = input<boolean>(false);
  readonly required = input<boolean>(false);

  readonly valueChange = output<string>();

  protected onInputChange(event: Event): void {
    const val = (event.target as HTMLTextAreaElement).value;
    this.valueChange.emit(val);
  }
}
