import { Component, ChangeDetectionStrategy, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

export interface AccordionItem {
  id: string;
  title: string;
  subtitle?: string;
  content: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-accordion',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="accordion-group">
      @for (item of items(); track item.id) {
        <div class="accordion-item" [class.open]="isOpen(item.id)">
          <button
            type="button"
            class="accordion-header"
            [attr.aria-expanded]="isOpen(item.id)"
            [disabled]="item.disabled"
            (click)="toggle(item.id)"
          >
            <div class="accordion-title-group">
              <span class="accordion-title">{{ item.title }}</span>
              @if (item.subtitle) {
                <span class="accordion-subtitle">{{ item.subtitle }}</span>
              }
            </div>
            <app-icon
              name="chevron-down"
              size="sm"
              [class]="'accordion-icon ' + (isOpen(item.id) ? 'rotate-180' : '')"
            />
          </button>
          @if (isOpen(item.id)) {
            <div class="accordion-body" role="region">
              <p class="accordion-content">{{ item.content }}</p>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .accordion-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    .accordion-item {
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      background-color: var(--bg-elevated);
      overflow: hidden;
      transition: all var(--duration-fast) var(--ease-fluent);

      &.open {
        border-color: var(--border-strong);
        box-shadow: var(--shadow-xs);
      }
    }
    .accordion-header {
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4);
      background: transparent;
      border: none;
      text-align: left;
      cursor: pointer;

      &:hover:not(:disabled) {
        background-color: var(--bg-tertiary);
      }

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }
    .accordion-title-group {
      display: flex;
      flex-direction: column;
    }
    .accordion-title {
      font-weight: 600;
      color: var(--text-primary);
      font-size: var(--text-body-md);
    }
    .accordion-subtitle {
      font-size: var(--text-body-sm);
      color: var(--text-muted);
    }
    .accordion-icon {
      transition: transform var(--duration-fast) var(--ease-fluent);
      &.rotate-180 { transform: rotate(180deg); }
    }
    .accordion-body {
      padding: 0 var(--space-4) var(--space-4) var(--space-4);
      border-top: 1px solid var(--border-subtle);
    }
    .accordion-content {
      margin: var(--space-3) 0 0 0;
      color: var(--text-secondary);
      font-size: var(--text-body-sm);
      line-height: var(--lh-body-sm);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccordionComponent {
  readonly items = input.required<AccordionItem[]>();
  readonly allowMultiple = input<boolean>(false);

  protected openIds = signal<Set<string>>(new Set());

  protected isOpen(id: string): boolean {
    return this.openIds().has(id);
  }

  protected toggle(id: string): void {
    this.openIds.update((set) => {
      const newSet = new Set(this.allowMultiple() ? set : []);
      if (set.has(id)) {
        newSet.delete(id);
      } else {
        newSet.add(id);
      }
      return newSet;
    });
  }
}
