import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';

@Component({
  selector: 'app-chip',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div [class]="chipClasses()" (click)="onChipClick($event)">
      @if (icon()) {
        <app-icon [name]="icon()!" size="xs" />
      }
      <span class="chip-label"><ng-content /></span>
      @if (removable()) {
        <button
          type="button"
          class="chip-remove"
          aria-label="Remove chip"
          (click)="onRemoveClick($event)"
        >
          <app-icon name="x" size="xs" />
        </button>
      }
    </div>
  `,
  styles: [`
    .chip {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-pill);
      font-size: var(--text-body-sm);
      font-weight: 500;
      background-color: var(--bg-tertiary);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
      transition: all var(--duration-fast) var(--ease-fluent);
      user-select: none;

      &.clickable {
        cursor: pointer;
        &:hover {
          background-color: var(--border-subtle);
          border-color: var(--border-strong);
        }
      }

      &.selected {
        background-color: var(--brand-primary-alpha);
        border-color: var(--brand-primary);
        color: var(--brand-primary);
      }

      .chip-label {
        line-height: 1;
      }

      .chip-remove {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: transparent;
        border: none;
        padding: 0;
        margin-left: var(--space-1);
        color: var(--text-muted);
        cursor: pointer;
        border-radius: var(--radius-circle);

        &:hover {
          color: var(--danger);
        }
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChipComponent {
  readonly icon = input<IconName>();
  readonly removable = input<boolean>(false);
  readonly selected = input<boolean>(false);
  readonly clickable = input<boolean>(false);

  readonly removed = output<void>();
  readonly chipClick = output<MouseEvent>();

  protected chipClasses = computed(() => {
    return [
      'chip',
      this.selected() ? 'selected' : '',
      this.clickable() ? 'clickable' : '',
    ].filter(Boolean).join(' ');
  });

  protected onChipClick(event: MouseEvent): void {
    if (this.clickable()) {
      this.chipClick.emit(event);
    }
  }

  protected onRemoveClick(event: MouseEvent): void {
    event.stopPropagation();
    this.removed.emit();
  }
}
