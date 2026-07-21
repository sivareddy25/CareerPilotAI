import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-checkbox',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <label class="checkbox-wrapper" [class.disabled]="disabled()">
      <input
        type="checkbox"
        [id]="id()"
        [checked]="checked()"
        [disabled]="disabled()"
        class="sr-only"
        (change)="onToggle($event)"
      />
      <span class="checkbox-box" [class.checked]="checked()" [class.indeterminate]="indeterminate()">
        @if (indeterminate()) {
          <span class="indeterminate-dash"></span>
        } @else if (checked()) {
          <app-icon name="check" size="xs" />
        }
      </span>
      @if (label()) {
        <span class="checkbox-label">{{ label() }}</span>
      }
    </label>
  `,
  styles: [`
    .checkbox-wrapper {
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
    .checkbox-box {
      width: 18px;
      height: 18px;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-strong);
      background-color: var(--bg-elevated);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #ffffff;
      transition: all var(--duration-fast) var(--ease-fluent);

      &.checked, &.indeterminate {
        background-color: var(--brand-primary);
        border-color: var(--brand-primary);
      }
    }
    .indeterminate-dash {
      width: 10px;
      height: 2px;
      background-color: #ffffff;
      border-radius: 1px;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckboxComponent {
  readonly id = input<string>(`checkbox-${Math.random().toString(36).substr(2, 9)}`);
  readonly label = input<string>();
  readonly checked = input<boolean>(false);
  readonly indeterminate = input<boolean>(false);
  readonly disabled = input<boolean>(false);

  readonly checkedChange = output<boolean>();

  protected onToggle(event: Event): void {
    if (!this.disabled()) {
      const isChecked = (event.target as HTMLInputElement).checked;
      this.checkedChange.emit(isChecked);
    }
  }
}
