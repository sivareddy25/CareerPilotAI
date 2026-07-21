import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="pagination-wrapper" aria-label="Pagination Navigation">
      <div class="pagination-info">
        Showing <strong>{{ startItem() }}</strong> to <strong>{{ endItem() }}</strong> of <strong>{{ totalItems() }}</strong> entries
      </div>
      <div class="pagination-controls">
        <button
          type="button"
          class="page-btn"
          [disabled]="currentPage() <= 1"
          aria-label="Previous Page"
          (click)="setPage(currentPage() - 1)"
        >
          <app-icon name="chevron-left" size="sm" />
        </button>

        @for (page of pages(); track page) {
          @if (page === -1) {
            <span class="page-ellipsis">...</span>
          } @else {
            <button
              type="button"
              [class.active]="currentPage() === page"
              class="page-btn page-number"
              [attr.aria-current]="currentPage() === page ? 'page' : null"
              (click)="setPage(page)"
            >
              {{ page }}
            </button>
          }
        }

        <button
          type="button"
          class="page-btn"
          [disabled]="currentPage() >= totalPages()"
          aria-label="Next Page"
          (click)="setPage(currentPage() + 1)"
        >
          <app-icon name="chevron-right" size="sm" />
        </button>
      </div>
    </div>
  `,
  styles: [`
    .pagination-wrapper {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      padding: var(--space-3) 0;
      flex-wrap: wrap;
    }
    .pagination-info {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
    }
    .pagination-controls {
      display: flex;
      align-items: center;
      gap: var(--space-1);
    }
    .page-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 36px;
      height: 36px;
      padding: 0 var(--space-2);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background-color: var(--bg-elevated);
      color: var(--text-primary);
      font-size: var(--text-body-sm);
      font-weight: 500;
      cursor: pointer;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover:not(:disabled) {
        background-color: var(--bg-tertiary);
        border-color: var(--border-strong);
      }

      &.active {
        background-color: var(--brand-primary);
        border-color: var(--brand-primary);
        color: #ffffff;
      }

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
    }
    .page-ellipsis {
      padding: 0 var(--space-2);
      color: var(--text-muted);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationComponent {
  readonly currentPage = input<number>(1);
  readonly pageSize = input<number>(10);
  readonly totalItems = input<number>(0);

  readonly pageChange = output<number>();

  protected totalPages = computed(() => {
    return Math.ceil(this.totalItems() / this.pageSize()) || 1;
  });

  protected startItem = computed(() => {
    if (this.totalItems() === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize() + 1;
  });

  protected endItem = computed(() => {
    return Math.min(this.currentPage() * this.pageSize(), this.totalItems());
  });

  protected pages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const result: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) result.push(i);
    } else {
      result.push(1);
      if (current > 3) result.push(-1);
      
      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);

      for (let i = start; i <= end; i++) result.push(i);

      if (current < total - 2) result.push(-1);
      result.push(total);
    }
    return result;
  });

  protected setPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.currentPage()) {
      this.pageChange.emit(page);
    }
  }
}
