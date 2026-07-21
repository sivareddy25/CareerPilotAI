import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { JobService } from '../../../core/services/job.service';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  ChipComponent,
  IconComponent,
  SpinnerComponent,
  BreadcrumbComponent,
  BreadcrumbItem,
} from '../../../shared/components';

@Component({
  selector: 'app-job-details',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    ChipComponent,
    IconComponent,
    SpinnerComponent,
    BreadcrumbComponent,
  ],
  template: `
    <div class="details-container">
      <app-breadcrumb [items]="breadcrumbs" />

      @if (isLoading()) {
        <div class="loading-state">
          <app-spinner size="lg" message="Loading job posting details..." />
        </div>
      } @else if (job(); as j) {
        <div class="details-grid">
          <!-- Main Job Detail Content -->
          <main class="main-content">
            <app-card>
              <div class="header-row">
                <div>
                  <h1 class="job-title">{{ j.title }}</h1>
                  <div class="company-sub-bar">
                    <span class="company-name">{{ j.company.name }}</span>
                    <span class="dot">•</span>
                    <span class="source-tag">Ingested via {{ j.sourceName }}</span>
                  </div>
                </div>

                @if (j.applyUrl) {
                  <a [href]="j.applyUrl" target="_blank" rel="noopener noreferrer" class="apply-link">
                    <app-button variant="primary" size="lg">
                      <app-icon name="external-link" size="sm" /> Apply on Provider Page
                    </app-button>
                  </a>
                }
              </div>

              <div class="meta-pills-row">
                <app-badge variant="success">{{ j.location.displayLocation }}</app-badge>
                <app-badge variant="secondary">{{ j.salary.formattedRange }}</app-badge>
                <app-badge variant="info">{{ j.employmentType }}</app-badge>
                <app-badge variant="warning">{{ j.experienceLevel }}</app-badge>
              </div>

              <div class="section-block">
                <h3 class="section-title">Job Overview</h3>
                <p class="section-text">{{ j.description }}</p>
              </div>

              @if (j.requirements) {
                <div class="section-block">
                  <h3 class="section-title">Requirements & Qualifications</h3>
                  <p class="section-text">{{ j.requirements }}</p>
                </div>
              }

              @if (j.responsibilities) {
                <div class="section-block">
                  <h3 class="section-title">Responsibilities</h3>
                  <p class="section-text">{{ j.responsibilities }}</p>
                </div>
              }

              @if (j.benefits) {
                <div class="section-block">
                  <h3 class="section-title">Benefits & Perks</h3>
                  <p class="section-text">{{ j.benefits }}</p>
                </div>
              }

              <div class="section-block">
                <h3 class="section-title">Required Skills</h3>
                <div class="chips-flex">
                  @for (skill of j.skills; track skill) {
                    <app-chip>{{ skill }}</app-chip>
                  }
                </div>
              </div>
            </app-card>
          </main>

          <!-- Sidebar Company & Metadata Card -->
          <aside class="sidebar-content">
            <app-card title="About Company">
              <h3 class="side-company-name">{{ j.company.name }}</h3>
              <p class="side-company-desc">{{ j.company.description }}</p>

              @if (j.company.websiteUrl) {
                <a [href]="j.company.websiteUrl" target="_blank" rel="noopener noreferrer" class="company-link">
                  <app-button variant="outline" [fullWidth]="true">
                    <app-icon name="building" size="sm" /> Company Website
                  </app-button>
                </a>
              }
            </app-card>
          </aside>
        </div>
      }
    </div>
  `,
  styles: [`
    .details-container {
      padding: var(--space-6);
      max-width: 1200px;
      margin: 0 auto;
    }
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-12) 0;
    }
    .details-grid {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: var(--space-6);
      margin-top: var(--space-6);
      align-items: start;
    }
    @media (max-width: 992px) {
      .details-grid { grid-template-columns: 1fr; }
    }
    .header-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: var(--space-4);
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--border-color);
      margin-bottom: var(--space-4);
    }
    .job-title {
      font-size: var(--text-h2);
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 var(--space-2) 0;
    }
    .company-sub-bar {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
    }
    .company-name { font-weight: 600; color: var(--brand-primary); }
    .apply-link { text-decoration: none; }
    .meta-pills-row {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-2);
      margin-bottom: var(--space-6);
    }
    .section-block {
      margin-bottom: var(--space-6);
    }
    .section-title {
      font-size: var(--text-h4);
      font-weight: 600;
      color: var(--text-primary);
      margin: 0 0 var(--space-2) 0;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: var(--space-1);
    }
    .section-text {
      font-size: var(--text-body-md);
      color: var(--text-secondary);
      line-height: 1.6;
      margin: 0;
      white-space: pre-line;
    }
    .chips-flex {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-2);
      margin-top: var(--space-2);
    }
    .side-company-name {
      font-size: var(--text-h3);
      margin: 0 0 var(--space-2) 0;
    }
    .side-company-desc {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      margin: 0 0 var(--space-4) 0;
    }
    .company-link { text-decoration: none; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobDetailsComponent implements OnInit {
  private readonly jobService = inject(JobService);
  private readonly route = inject(ActivatedRoute);

  protected readonly job = this.jobService.activeJob;
  protected readonly isLoading = this.jobService.isLoading;

  protected readonly breadcrumbs: BreadcrumbItem[] = [
    { label: 'Job Aggregator', url: '/jobs' },
    { label: 'Job Details', url: '' },
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.jobService.getJobById(id).subscribe();
    }
  }
}
