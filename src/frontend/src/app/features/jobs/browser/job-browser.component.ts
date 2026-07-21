import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { JobService } from '../../../core/services/job.service';
import { JobFilterParams } from '../../../core/models/job.models';
import {
  PageHeaderComponent,
  SearchBarComponent,
  ButtonComponent,
  PaginationComponent,
  SpinnerComponent,
  EmptyStateComponent,
  IconComponent,
  BadgeComponent,
} from '../../../shared/components';
import { JobFilterPanelComponent } from '../filter-panel/job-filter-panel.component';
import { JobCardComponent } from '../card/job-card.component';

export type ViewMode = 'grid' | 'list';

@Component({
  selector: 'app-job-browser',
  standalone: true,
  imports: [
    CommonModule,
    PageHeaderComponent,
    SearchBarComponent,
    ButtonComponent,
    PaginationComponent,
    SpinnerComponent,
    EmptyStateComponent,
    IconComponent,
    BadgeComponent,
    JobFilterPanelComponent,
    JobCardComponent,
  ],
  template: `
    <div class="job-browser-container">
      <app-page-header
        title="Job Search & Aggregator"
        subtitle="Explore aggregated opportunities ingested and normalized across Greenhouse, Lever, Ashby, Workday, SmartRecruiters & Career Pages."
      >
        <div header-actions class="browser-actions">
          <div class="view-toggle">
            <button
              type="button"
              class="toggle-btn"
              [class.active]="viewMode() === 'grid'"
              (click)="viewMode.set('grid')"
              title="Grid View"
            >
              <app-icon name="grid" size="sm" />
            </button>
            <button
              type="button"
              class="toggle-btn"
              [class.active]="viewMode() === 'list'"
              (click)="viewMode.set('list')"
              title="List View"
            >
              <app-icon name="list" size="sm" />
            </button>
          </div>

          <app-button
            variant="outline"
            [disabled]="isSyncing()"
            (btnClick)="onSyncTrigger()"
          >
            <app-icon name="refresh-cw" size="sm" />
            {{ isSyncing() ? 'Synchronizing Providers...' : 'Trigger ATS Sync' }}
          </app-button>
        </div>
      </app-page-header>

      <div class="search-bar-wrapper">
        <app-search-bar
          placeholder="Search job titles, skills, or companies..."
          [value]="searchTerm()"
          (searchChange)="onSearch($event)"
        />
      </div>

      <div class="browser-body">
        <!-- Sidebar Filter Panel -->
        <aside class="sidebar-column">
          <app-job-filter-panel (filterChange)="onFilterChange($event)" />
        </aside>

        <!-- Main Job List Content -->
        <main class="content-column">
          @if (isLoading()) {
            <div class="loading-state">
              <app-spinner size="lg" message="Ingesting & querying normalized job pool..." />
            </div>
          } @else if (jobs().length === 0) {
            <app-empty-state
              title="No Jobs Found"
              description="No job postings match your search filters. Try adjusting keywords or location."
              icon="search"
            />
          } @else {
            <div [class]="viewMode() === 'grid' ? 'jobs-grid' : 'jobs-list'">
              @for (job of jobs(); track job.id) {
                @if (viewMode() === 'grid') {
                  <app-job-card [job]="job" (viewDetails)="navigateToDetails($event)" />
                } @else {
                  <div class="list-item-card" (click)="navigateToDetails(job.id)">
                    <div class="list-company-logo">
                      {{ job.company.name.charAt(0) }}
                    </div>
                    <div class="list-body">
                      <h4 class="list-job-title">{{ job.title }}</h4>
                      <p class="list-company-meta">
                        <strong>{{ job.company.name }}</strong> • {{ job.location.displayLocation }} • {{ job.salary.formattedRange }}
                      </p>
                    </div>
                    <div class="list-actions">
                      <app-badge variant="success">94% Match</app-badge>
                      <app-button variant="outline" size="sm">
                        View Position
                      </app-button>
                    </div>
                  </div>
                }
              }
            </div>

            <div class="pagination-wrapper">
              <app-pagination
                [currentPage]="pageNumber()"
                [pageSize]="12"
                [totalItems]="totalCount()"
                (pageChange)="onPageChange($event)"
              />
            </div>
          }
        </main>
      </div>
    </div>
  `,
  styles: [`
    .job-browser-container {
      padding: var(--space-6);
      max-width: 1400px;
      margin: 0 auto;
    }
    .browser-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }
    .view-toggle {
      display: flex;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      overflow: hidden;
      background-color: var(--bg-tertiary);
    }
    .toggle-btn {
      padding: var(--space-2) var(--space-3);
      border: none;
      background: transparent;
      color: var(--text-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover { color: var(--text-primary); }
      &.active {
        background-color: var(--bg-elevated);
        color: var(--brand-primary);
        box-shadow: var(--shadow-xs);
      }
    }
    .search-bar-wrapper {
      margin: var(--space-6) 0;
    }
    .browser-body {
      display: grid;
      grid-template-columns: 300px 1fr;
      gap: var(--space-6);
      align-items: start;
    }
    @media (max-width: 992px) {
      .browser-body { grid-template-columns: 1fr; }
    }
    .jobs-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: var(--space-6);
      margin-bottom: var(--space-6);
    }
    .jobs-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      margin-bottom: var(--space-6);
    }
    .list-item-card {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-4);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      cursor: pointer;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover {
        border-color: var(--brand-primary);
        transform: translateY(-2px);
        box-shadow: var(--shadow-md);
      }
    }
    .list-company-logo {
      width: 44px;
      height: 44px;
      border-radius: var(--radius-md);
      background-color: var(--brand-primary-alpha);
      color: var(--brand-primary);
      font-weight: 700;
      font-size: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .list-body { flex: 1; }
    .list-job-title {
      font-size: var(--text-h4);
      font-weight: 600;
      margin: 0 0 var(--space-1) 0;
      color: var(--text-primary);
    }
    .list-company-meta {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      margin: 0;
    }
    .list-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-12) 0;
    }
    .pagination-wrapper {
      display: flex;
      justify-content: center;
      margin-top: var(--space-6);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobBrowserComponent implements OnInit {
  private readonly jobService = inject(JobService);
  private readonly router = inject(Router);

  protected readonly jobs = this.jobService.jobs;
  protected readonly totalCount = this.jobService.totalCount;
  protected readonly pageNumber = this.jobService.pageNumber;
  protected readonly isLoading = this.jobService.isLoading;
  protected readonly isSyncing = this.jobService.isSyncing;

  protected readonly viewMode = signal<ViewMode>('grid');
  protected searchTerm = signal<string>('');

  ngOnInit(): void {
    this.jobService.loadJobs().subscribe();
  }

  protected onSearch(term: string): void {
    this.searchTerm.set(term);
    this.jobService.loadJobs({ search: term, pageNumber: 1 }).subscribe();
  }

  protected onFilterChange(filter: JobFilterParams): void {
    this.jobService.loadJobs({ ...filter, search: this.searchTerm() }).subscribe();
  }

  protected onPageChange(page: number): void {
    this.jobService.loadJobs({ pageNumber: page }).subscribe();
  }

  protected onSyncTrigger(): void {
    this.jobService.synchronize().subscribe();
  }

  protected navigateToDetails(id: string): void {
    this.router.navigate(['/jobs', id]);
  }
}
