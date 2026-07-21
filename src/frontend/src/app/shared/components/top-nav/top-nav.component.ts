import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

export interface TopNavItem {
  label: string;
  url: string;
}

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="top-nav" aria-label="Header Menu">
      @for (item of items(); track item.url) {
        <a
          [routerLink]="item.url"
          routerLinkActive="active"
          class="nav-item-link"
        >
          {{ item.label }}
        </a>
      }
    </nav>
  `,
  styles: [`
    .top-nav {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    .nav-item-link {
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-secondary);
      text-decoration: none;
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-md);
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover {
        color: var(--text-primary);
        background-color: var(--bg-tertiary);
      }

      &.active {
        color: var(--brand-primary);
        font-weight: 600;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopNavComponent {
  readonly items = input.required<TopNavItem[]>();
}
