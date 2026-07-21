import { Component, ChangeDetectionStrategy, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';

export interface DropdownItem {
  id: string;
  label: string;
  icon?: IconName;
  danger?: boolean;
  disabled?: boolean;
}

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="dropdown-wrapper">
      <div (click)="toggle()">
        <ng-content select="[trigger]" />
      </div>

      @if (isOpen()) {
        <div class="dropdown-menu" role="menu">
          @for (item of items(); track item.id) {
            <button
              type="button"
              role="menuitem"
              [disabled]="item.disabled"
              [class.danger]="item.danger"
              class="dropdown-item"
              (click)="onSelect(item.id)"
            >
              @if (item.icon) {
                <app-icon [name]="item.icon" size="sm" />
              }
              <span>{{ item.label }}</span>
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .dropdown-wrapper {
      position: relative;
      display: inline-block;
    }
    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      margin-top: var(--space-1);
      min-width: 180px;
      padding: var(--space-1);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      z-index: var(--z-dropdown);
      animation: scaleIn var(--duration-fast) var(--ease-fluent);
    }
    .dropdown-item {
      width: 100%;
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-3);
      font-size: var(--text-body-sm);
      color: var(--text-primary);
      background: transparent;
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      text-align: left;
      transition: background-color var(--duration-fast) var(--ease-fluent);

      &:hover:not(:disabled) {
        background-color: var(--bg-tertiary);
      }

      &.danger {
        color: var(--danger);
        &:hover:not(:disabled) {
          background-color: var(--danger-bg);
        }
      }

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownComponent {
  readonly items = input.required<DropdownItem[]>();

  readonly itemSelect = output<string>();

  protected isOpen = signal<boolean>(false);

  protected toggle(): void {
    this.isOpen.update((v) => !v);
  }

  protected onSelect(id: string): void {
    this.itemSelect.emit(id);
    this.isOpen.set(false);
  }
}
