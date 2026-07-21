import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="search-bar-container">
      <app-icon name="search" size="sm" class="search-icon" />
      <input
        type="text"
        [value]="value()"
        [placeholder]="placeholder()"
        class="search-input"
        (input)="onInput($event)"
      />
      @if (value()) {
        <button type="button" class="clear-btn" aria-label="Clear search" (click)="clear()">
          <app-icon name="x" size="xs" />
        </button>
      }
      @if (shortcutKey()) {
        <kbd class="shortcut-badge">{{ shortcutKey() }}</kbd>
      }
    </div>
  `,
  styles: [`
    .search-bar-container {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
    }
    .search-icon {
      position: absolute;
      left: 12px;
      color: var(--text-muted);
      pointer-events: none;
    }
    .search-input {
      width: 100%;
      height: 40px;
      padding: var(--space-2) var(--space-8) var(--space-2) 38px;
      font-size: var(--text-body-sm);
      font-family: var(--font-sans);
      color: var(--text-primary);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-sizing: border-box;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:focus {
        outline: none;
        border-color: var(--border-focus);
        box-shadow: var(--shadow-focus);
      }
    }
    .clear-btn {
      position: absolute;
      right: 12px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 2px;
      border-radius: var(--radius-circle);

      &:hover { color: var(--text-primary); }
    }
    .shortcut-badge {
      position: absolute;
      right: 12px;
      padding: 2px 6px;
      font-size: var(--text-caption);
      font-family: var(--font-mono);
      color: var(--text-muted);
      background-color: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xs);
      pointer-events: none;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBarComponent {
  readonly value = input<string>('');
  readonly placeholder = input<string>('Search...');
  readonly shortcutKey = input<string>();

  readonly searchChange = output<string>();
  readonly searchSubmit = output<string>();

  protected onInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchChange.emit(val);
  }

  protected clear(): void {
    this.searchChange.emit('');
  }
}
