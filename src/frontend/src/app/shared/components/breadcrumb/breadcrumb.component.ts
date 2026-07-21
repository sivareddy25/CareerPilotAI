import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

export interface BreadcrumbItem {
  label: string;
  url?: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <nav aria-label="Breadcrumb" class="breadcrumb-nav">
      <ol class="breadcrumb-list">
        @for (item of items(); track item.label; let last = $last) {
          <li class="breadcrumb-item" [attr.aria-current]="last ? 'page' : null">
            @if (!last && item.url) {
              <a [href]="item.url" class="breadcrumb-link">{{ item.label }}</a>
              <app-icon name="chevron-right" size="xs" class="breadcrumb-separator" />
            } @else {
              <span class="breadcrumb-current">{{ item.label }}</span>
            }
          </li>
        }
      </ol>
    </nav>
  `,
  styles: [`
    .breadcrumb-nav {
      display: flex;
      align-items: center;
    }
    .breadcrumb-list {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      list-style: none;
      padding: 0;
      margin: 0;
    }
    .breadcrumb-item {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-body-sm);
    }
    .breadcrumb-link {
      color: var(--text-secondary);
      text-decoration: none;
      transition: color var(--duration-fast) var(--ease-fluent);

      &:hover {
        color: var(--brand-primary);
        text-decoration: underline;
      }
    }
    .breadcrumb-separator {
      color: var(--text-muted);
    }
    .breadcrumb-current {
      color: var(--text-primary);
      font-weight: 600;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BreadcrumbComponent {
  readonly items = input.required<BreadcrumbItem[]>();
}
