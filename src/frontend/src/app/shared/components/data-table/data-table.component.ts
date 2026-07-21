import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';
import { CheckboxComponent } from '../checkbox/checkbox.component';
import { BadgeComponent, BadgeVariant } from '../badge/badge.component';

export interface ColumnDef {
  key: string;
  header: string;
  sortable?: boolean;
  type?: 'text' | 'badge' | 'date';
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, IconComponent, CheckboxComponent, BadgeComponent],
  template: `
    <div class="data-table-container">
      <table class="data-table">
        <thead>
          <tr>
            @if (selectable()) {
              <th class="table-th-select">
                <app-checkbox (checkedChange)="toggleAll($event)" />
              </th>
            }
            @for (col of columns(); track col.key) {
              <th
                class="table-th"
                [class.sortable]="col.sortable"
                (click)="col.sortable ? onSort(col.key) : null"
              >
                <div class="th-content">
                  <span>{{ col.header }}</span>
                  @if (col.sortable) {
                    <app-icon name="chevron-down" size="xs" class="sort-icon" />
                  }
                </div>
              </th>
            }
          </tr>
        </thead>
        <tbody>
          @if (data().length === 0) {
            <tr>
              <td [attr.colspan]="columns().length + (selectable() ? 1 : 0)" class="no-data">
                No records found.
              </td>
            </tr>
          } @else {
            @for (row of data(); track row.id || row) {
              <tr class="table-row">
                @if (selectable()) {
                  <td class="table-td-select">
                    <app-checkbox />
                  </td>
                }
                @for (col of columns(); track col.key) {
                  <td class="table-td">
                    @if (col.type === 'badge') {
                      <app-badge [variant]="getBadgeVariant(row[col.key])">{{ row[col.key] }}</app-badge>
                    } @else {
                      {{ row[col.key] }}
                    }
                  </td>
                }
              </tr>
            }
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .data-table-container {
      width: 100%;
      overflow-x: auto;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      background-color: var(--bg-elevated);
    }
    .data-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
      font-size: var(--text-body-sm);
    }
    thead {
      background-color: var(--bg-secondary);
      border-bottom: 1px solid var(--border-color);
    }
    .table-th {
      padding: var(--space-3) var(--space-4);
      font-weight: 600;
      color: var(--text-secondary);
      user-select: none;

      &.sortable {
        cursor: pointer;
        &:hover { color: var(--text-primary); }
      }
    }
    .th-content {
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }
    .sort-icon { color: var(--text-muted); }
    .table-th-select, .table-td-select {
      width: 40px;
      padding: var(--space-3) var(--space-3);
      text-align: center;
    }
    .table-row {
      border-bottom: 1px solid var(--border-subtle);
      transition: background-color var(--duration-fast) var(--ease-fluent);

      &:hover {
        background-color: var(--bg-tertiary);
      }

      &:last-child {
        border-bottom: none;
      }
    }
    .table-td {
      padding: var(--space-3) var(--space-4);
      color: var(--text-primary);
    }
    .no-data {
      padding: var(--space-6);
      text-align: center;
      color: var(--text-muted);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTableComponent {
  readonly columns = input.required<ColumnDef[]>();
  readonly data = input.required<any[]>();
  readonly selectable = input<boolean>(false);

  readonly sort = output<{ key: string; direction: 'asc' | 'desc' }>();

  private currentSortDir: 'asc' | 'desc' = 'asc';

  protected onSort(key: string): void {
    this.currentSortDir = this.currentSortDir === 'asc' ? 'desc' : 'asc';
    this.sort.emit({ key, direction: this.currentSortDir });
  }

  protected toggleAll(checked: boolean): void {
    // Select all handler
  }

  protected getBadgeVariant(val: string): BadgeVariant {
    const lower = String(val).toLowerCase();
    if (lower.includes('active') || lower.includes('success') || lower.includes('hired')) return 'success';
    if (lower.includes('pending') || lower.includes('warning') || lower.includes('review')) return 'warning';
    if (lower.includes('rejected') || lower.includes('danger') || lower.includes('failed')) return 'danger';
    return 'primary';
  }
}
