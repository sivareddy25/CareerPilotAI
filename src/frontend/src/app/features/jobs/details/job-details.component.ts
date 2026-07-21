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
  ProgressBarComponent,
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
    ProgressBarComponent,
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
          <!-- LEFT COLUMN: Description, Responsibilities & Requirements -->
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

          <!-- RIGHT COLUMN: AI Match Score, Resume Recommendation & Cover Letter -->
          <aside class="sidebar-content">
            <!-- AI Match Analysis Card -->
            <app-card title="AI Match & Resume Recommendation" class="ai-match-card">
              <div class="match-score-badge">
                <div class="score-number">94%</div>
                <app-badge variant="success">Strong Match</app-badge>
              </div>
              <app-progress-bar [value]="94" variant="success" />

              <div class="ai-insights-box">
                <h4><app-icon name="sparkles" size="sm" /> Key Skill Overlaps</h4>
                <ul>
                  <li>C# .NET 9 Clean Architecture & CQRS</li>
                  <li>Angular Signals & State Management</li>
                  <li>Playwright Automation & System Design</li>
                </ul>
              </div>

              <div class="action-buttons-stack">
                <app-button variant="primary" [fullWidth]="true" (btnClick)="navigateTo('/resumes/templates')">
                  <app-icon name="file-text" size="sm" /> Tailor Resume for Job
                </app-button>
                <app-button variant="outline" [fullWidth]="true" (btnClick)="navigateTo('/communication')">
                  <app-icon name="mail" size="sm" /> Generate Cover Letter
                </app-button>
              </div>
            </app-card>

            <!-- Company Card -->
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
      max-width: 1300px;
      margin: 0 auto;
    }
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-12) 0;
    }
    .details-grid {
      display: grid;
      grid-template-columns: 1fr 360px;
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

    .sidebar-content {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    .ai-match-card {
      border: 1px solid var(--ai-accent-border);
      background: linear-gradient(135deg, var(--bg-elevated) 0%, var(--bg-tertiary) 100%);
    }
    .match-score-badge {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: var(--space-3);
    }
    .score-number {
      font-size: 32px;
      font-weight: 800;
      color: var(--brand-primary);
    }
    .ai-insights-box {
      margin: var(--space-4) 0;
      padding: var(--space-3);
      border-radius: var(--radius-md);
      background-color: var(--ai-accent-bg);
      h4 {
        margin: 0 0 var(--space-2) 0;
        font-size: var(--text-body-sm);
        color: var(--ai-accent-text);
        display: flex;
        align-items: center;
        gap: var(--space-2);
      }
      ul {
        margin: 0;
        padding-left: var(--space-4);
        font-size: var(--text-caption);
        color: var(--text-secondary);
      }
    }
    .action-buttons-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      margin-top: var(--space-4);
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
  private readonly router = inject(Router);

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

  protected navigateTo(url: string): void {
    this.router.navigateByUrl(url);
  }
}
