import { Component, ChangeDetectionStrategy, input, output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FocusTrapDirective } from '../../directives/focus-trap.directive';
import { IconButtonComponent } from '../icon-button/icon-button.component';

export type DrawerPosition = 'left' | 'right';

@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule, FocusTrapDirective, IconButtonComponent],
  template: `
    @if (isOpen()) {
      <div class="drawer-backdrop" (click)="close()">
        <div
          [class]="'drawer-panel drawer-' + position()"
          appFocusTrap
          role="dialog"
          aria-modal="true"
          (click)="$event.stopPropagation()"
        >
          <div class="drawer-header">
            <h3 class="drawer-title">{{ title() }}</h3>
            <app-icon-button icon="x" size="sm" ariaLabel="Close drawer" (btnClick)="close()" />
          </div>
          <div class="drawer-body">
            <ng-content />
          </div>
          <div class="drawer-footer">
            <ng-content select="[drawer-footer]" />
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .drawer-backdrop {
      position: fixed;
      inset: 0;
      background-color: var(--bg-overlay);
      backdrop-filter: blur(2px);
      z-index: var(--z-drawer);
      animation: fadeIn var(--duration-fast) var(--ease-fluent);
    }
    .drawer-panel {
      position: fixed;
      top: 0;
      bottom: 0;
      width: 100%;
      max-width: 400px;
      background-color: var(--bg-elevated);
      border-color: var(--border-color);
      box-shadow: var(--shadow-2xl);
      display: flex;
      flex-direction: column;

      &.drawer-left {
        left: 0;
        border-right: 1px solid var(--border-color);
        animation: slideInLeft var(--duration-normal) var(--ease-fluent);
      }
      &.drawer-right {
        right: 0;
        border-left: 1px solid var(--border-color);
        animation: slideInRight var(--duration-normal) var(--ease-fluent);
      }
    }
    .drawer-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4) var(--space-6);
      border-bottom: 1px solid var(--border-subtle);
    }
    .drawer-title {
      margin: 0;
      font-size: var(--text-h4);
      font-weight: 600;
      color: var(--text-primary);
    }
    .drawer-body {
      padding: var(--space-6);
      overflow-y: auto;
      flex: 1;
    }
    .drawer-footer {
      padding: var(--space-4) var(--space-6);
      border-top: 1px solid var(--border-subtle);
      background-color: var(--bg-secondary);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DrawerComponent {
  readonly isOpen = input<boolean>(false);
  readonly title = input.required<string>();
  readonly position = input<DrawerPosition>('right');

  readonly closed = output<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }

  protected close(): void {
    this.closed.emit();
  }
}
