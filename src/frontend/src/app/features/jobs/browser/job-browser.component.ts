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
} from '../../../shared/components';
import { JobFilterPanelComponent } from '../filter-panel/job-filter-panel.component';
import { JobCardComponent } from '../card/job-card.component';

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
    JobFilterPanelComponent,
    JobCardComponent,
  ],
  template: `
    <div class="job-browser-container">
      <app-page-header
        title="Job Ingestion Engine"
        subtitle="Aggregated opportunities ingested and normalized across Greenhouse, Lever, Ashby, Workday, SmartRecruiters & Career Pages."
      >
        <div header-actions class="browser-actions">
          <app-button
            variant="outline"
            [disabled]="isSyncing()"
            (btnClick)="onSyncTrigger()"
          >
            <app-icon name="refresh-cw" size="sm" />
            {{ isSyncing() ? 'Synchronizing ATS Providers...' : 'Trigger ATS Sync' }}
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
              description="No job postings match your current filter parameters."
              icon="search"
            />
          } @else {
            <div class="jobs-grid">
              @for (job of jobs(); track job.id) {
                <app-job-card [job]="job" (viewDetails)="navigateToDetails($event)" />
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
      gap: var(--space-3);
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
