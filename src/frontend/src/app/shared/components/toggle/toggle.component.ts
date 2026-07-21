import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <label class="toggle-wrapper" [class.disabled]="disabled()">
      <input
        type="checkbox"
        [id]="id()"
        [checked]="checked()"
        [disabled]="disabled()"
        class="sr-only"
        (change)="onToggle($event)"
      />
      <span class="toggle-track" [class.checked]="checked()">
        <span class="toggle-thumb"></span>
      </span>
      @if (label()) {
        <span class="toggle-label">{{ label() }}</span>
      }
    </label>
  `,
  styles: [`
    .toggle-wrapper {
      display: inline-flex;
      align-items: center;
      gap: var(--space-3);
      cursor: pointer;
      user-select: none;
      font-size: var(--text-body-sm);
      color: var(--text-primary);

      &.disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }
    .toggle-track {
      width: 44px;
      height: 24px;
      border-radius: var(--radius-pill);
      background-color: var(--border-strong);
      position: relative;
      transition: background-color var(--duration-fast) var(--ease-fluent);

      &.checked {
        background-color: var(--brand-primary);
        .toggle-thumb {
          transform: translateX(20px);
        }
      }
    }
    .toggle-thumb {
      position: absolute;
      top: 2px;
      left: 2px;
      width: 20px;
      height: 20px;
      border-radius: var(--radius-circle);
      background-color: #ffffff;
      box-shadow: var(--shadow-xs);
      transition: transform var(--duration-fast) var(--ease-fluent);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToggleComponent {
  readonly id = input<string>(`toggle-${Math.random().toString(36).substr(2, 9)}`);
  readonly label = input<string>();
  readonly checked = input<boolean>(false);
  readonly disabled = input<boolean>(false);

  readonly checkedChange = output<boolean>();

  protected onToggle(event: Event): void {
    if (!this.disabled()) {
      const isChecked = (event.target as HTMLInputElement).checked;
      this.checkedChange.emit(isChecked);
    }
  }
}
