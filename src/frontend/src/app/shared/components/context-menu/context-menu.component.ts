import { Component, ChangeDetectionStrategy, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';

export interface ContextMenuItem {
  id: string;
  label: string;
  icon?: IconName;
  danger?: boolean;
}

@Component({
  selector: 'app-context-menu',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="context-target" (contextmenu)="onContextMenu($event)">
      <ng-content />

      @if (isOpen()) {
        <div
          class="context-menu-popover"
          [style.top.px]="pos().y"
          [style.left.px]="pos().x"
          (click)="$event.stopPropagation()"
        >
          @for (item of items(); track item.id) {
            <button
              type="button"
              [class.danger]="item.danger"
              class="menu-item"
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
    .context-target {
      position: relative;
      display: inline-block;
      width: 100%;
    }
    .context-menu-popover {
      position: fixed;
      min-width: 160px;
      padding: var(--space-1);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-xl);
      z-index: var(--z-popover);
      animation: scaleIn var(--duration-fast) var(--ease-fluent);
    }
    .menu-item {
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

      &:hover { background-color: var(--bg-tertiary); }
      &.danger { color: var(--danger); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContextMenuComponent {
  readonly items = input.required<ContextMenuItem[]>();

  readonly itemSelect = output<string>();

  protected isOpen = signal<boolean>(false);
  protected pos = signal<{ x: number; y: number }>({ x: 0, y: 0 });

  protected onContextMenu(event: MouseEvent): void {
    event.preventDefault();
    this.pos.set({ x: event.clientX, y: event.clientY });
    this.isOpen.set(true);

    const closeListener = () => {
      this.isOpen.set(false);
      window.removeEventListener('click', closeListener);
    };
    window.addEventListener('click', closeListener);
  }

  protected onSelect(id: string): void {
    this.itemSelect.emit(id);
    this.isOpen.set(false);
  }
}
