import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TabItem {
  id: string;
  label: string;
  icon?: string;
  badge?: string | number;
  disabled?: boolean;
}

@Component({
  selector: 'app-tabs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="tabs-container">
      <div class="tabs-list" role="tablist" [attr.aria-label]="ariaLabel()">
        @for (tab of items(); track tab.id) {
          <button
            type="button"
            role="tab"
            [id]="'tab-' + tab.id"
            [attr.aria-selected]="activeId() === tab.id"
            [attr.aria-controls]="'panel-' + tab.id"
            [tabIndex]="activeId() === tab.id ? 0 : -1"
            [disabled]="tab.disabled"
            [class.active]="activeId() === tab.id"
            class="tab-btn"
            (click)="selectTab(tab.id)"
          >
            <span>{{ tab.label }}</span>
            @if (tab.badge !== undefined) {
              <span class="tab-badge">{{ tab.badge }}</span>
            }
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    .tabs-container {
      border-bottom: 1px solid var(--border-color);
      margin-bottom: var(--space-4);
    }
    .tabs-list {
      display: flex;
      gap: var(--space-2);
      overflow-x: auto;

      &::-webkit-scrollbar { display: none; }
    }
    .tab-btn {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-3) var(--space-4);
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-secondary);
      background: transparent;
      border: none;
      border-bottom: 2px solid transparent;
      cursor: pointer;
      white-space: nowrap;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover:not(:disabled) {
        color: var(--text-primary);
        border-bottom-color: var(--border-strong);
      }

      &.active {
        color: var(--brand-primary);
        border-bottom-color: var(--brand-primary);
        font-weight: 600;
      }

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }

      .tab-badge {
        padding: 2px 6px;
        border-radius: var(--radius-pill);
        font-size: var(--text-caption);
        background-color: var(--bg-tertiary);
        color: var(--text-secondary);
      }

      &.active .tab-badge {
        background-color: var(--brand-primary-alpha);
        color: var(--brand-primary);
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TabsComponent {
  readonly items = input.required<TabItem[]>();
  readonly activeId = input.required<string>();
  readonly ariaLabel = input<string>('Tabs');

  readonly tabChange = output<string>();

  protected selectTab(id: string): void {
    this.tabChange.emit(id);
  }
}
