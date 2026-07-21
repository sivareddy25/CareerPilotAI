import { Component, ChangeDetectionStrategy, input, output, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';
import { FocusTrapDirective } from '../../directives/focus-trap.directive';

export interface CommandItem {
  id: string;
  category: string;
  title: string;
  description?: string;
  icon?: IconName;
  shortcut?: string;
}

@Component({
  selector: 'app-command-bar',
  standalone: true,
  imports: [CommonModule, IconComponent, FocusTrapDirective],
  template: `
    @if (isOpen()) {
      <div class="command-backdrop" (click)="close()">
        <div class="command-card" appFocusTrap (click)="$event.stopPropagation()">
          <div class="command-search-header">
            <app-icon name="search" size="md" class="search-icon" />
            <input
              type="text"
              [value]="query()"
              placeholder="Type a command or search..."
              class="command-input"
              (input)="onQueryChange($event)"
              #searchInput
            />
            <kbd class="esc-badge">ESC</kbd>
          </div>

          <div class="command-results">
            @if (filteredItems().length === 0) {
              <div class="no-results">No matching commands found.</div>
            } @else {
              @for (category of categories(); track category) {
                <div class="command-group">
                  <div class="group-header">{{ category }}</div>
                  @for (item of getCategoryItems(category); track item.id) {
                    <button
                      type="button"
                      class="command-item-btn"
                      (click)="onSelect(item)"
                    >
                      @if (item.icon) {
                        <app-icon [name]="item.icon" size="sm" />
                      }
                      <div class="item-text">
                        <span class="item-title">{{ item.title }}</span>
                        @if (item.description) {
                          <span class="item-desc">{{ item.description }}</span>
                        }
                      </div>
                      @if (item.shortcut) {
                        <kbd class="shortcut-badge">{{ item.shortcut }}</kbd>
                      }
                    </button>
                  }
                </div>
              }
            }
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .command-backdrop {
      position: fixed;
      inset: 0;
      background-color: var(--bg-overlay);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: flex-start;
      justify-content: center;
      padding-top: 10vh;
      z-index: var(--z-modal);
      animation: fadeIn var(--duration-fast) var(--ease-fluent);
    }
    .command-card {
      width: 100%;
      max-width: 600px;
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-2xl);
      overflow: hidden;
      animation: scaleIn var(--duration-fast) var(--ease-fluent);
    }
    .command-search-header {
      display: flex;
      align-items: center;
      padding: var(--space-3) var(--space-4);
      border-bottom: 1px solid var(--border-subtle);
    }
    .search-icon { color: var(--text-muted); margin-right: var(--space-3); }
    .command-input {
      flex: 1;
      height: 36px;
      border: none;
      background: transparent;
      font-size: var(--text-body-lg);
      font-family: var(--font-sans);
      color: var(--text-primary);
      &:focus { outline: none; }
    }
    .esc-badge {
      padding: 2px 6px;
      font-size: var(--text-caption);
      font-family: var(--font-mono);
      color: var(--text-muted);
      background-color: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xs);
    }
    .command-results {
      max-height: 360px;
      overflow-y: auto;
      padding: var(--space-2);
    }
    .command-group {
      margin-bottom: var(--space-3);
    }
    .group-header {
      padding: var(--space-1) var(--space-3);
      font-size: var(--text-caption);
      font-weight: 600;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .command-item-btn {
      width: 100%;
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2) var(--space-3);
      background: transparent;
      border: none;
      border-radius: var(--radius-md);
      color: var(--text-primary);
      cursor: pointer;
      text-align: left;
      transition: background-color var(--duration-fast) var(--ease-fluent);

      &:hover, &:focus {
        background-color: var(--bg-tertiary);
        outline: none;
      }
    }
    .item-text {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
    .item-title { font-size: var(--text-body-sm); font-weight: 500; }
    .item-desc { font-size: var(--text-caption); color: var(--text-muted); }
    .shortcut-badge {
      padding: 2px 6px;
      font-size: var(--text-caption);
      font-family: var(--font-mono);
      color: var(--text-muted);
      background-color: var(--bg-tertiary);
      border-radius: var(--radius-xs);
    }
    .no-results {
      padding: var(--space-6);
      text-align: center;
      color: var(--text-muted);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommandBarComponent {
  readonly isOpen = input<boolean>(false);
  readonly items = input<CommandItem[]>([
    { id: '1', category: 'General', title: 'Switch Theme', description: 'Toggle between light and dark mode', icon: 'sun', shortcut: 'Ctrl+T' },
    { id: '2', category: 'General', title: 'Open Settings', description: 'Manage application preferences', icon: 'settings', shortcut: 'Ctrl+,' },
    { id: '3', category: 'Navigation', title: 'Go to Design System', description: 'Browse all UI components', icon: 'grid' },
    { id: '4', category: 'Navigation', title: 'Go to Home', description: 'Return to dashboard', icon: 'user' },
  ]);

  readonly closed = output<void>();
  readonly itemSelect = output<CommandItem>();

  protected query = signal<string>('');

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }

  protected filteredItems = computed(() => {
    const q = this.query().toLowerCase().trim();
    if (!q) return this.items();
    return this.items().filter(
      (item) =>
        item.title.toLowerCase().includes(q) ||
        item.category.toLowerCase().includes(q) ||
        item.description?.toLowerCase().includes(q)
    );
  });

  protected categories = computed(() => {
    const cats = new Set<string>();
    this.filteredItems().forEach((item) => cats.add(item.category));
    return Array.from(cats);
  });

  protected getCategoryItems(cat: string): CommandItem[] {
    return this.filteredItems().filter((item) => item.category === cat);
  }

  protected onQueryChange(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  protected onSelect(item: CommandItem): void {
    this.itemSelect.emit(item);
    this.close();
  }

  protected close(): void {
    this.query.set('');
    this.closed.emit();
  }
}
