import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  IconComponent,
  PageHeaderComponent,
  ProgressBarComponent,
  SpinnerComponent,
} from '../../shared/components';

@Component({
  selector: 'app-executive-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent,
    PageHeaderComponent,
    ProgressBarComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="dashboard-container">
      <app-page-header
        title="CareerPilot AI Executive Hub"
        subtitle="Centralized SaaS dashboard for job ingestion, AI resume optimization, ATS tracking, and interview analytics."
      >
        <div header-actions class="dashboard-actions">
          <app-button variant="outline" (btnClick)="navigateTo('/resumes/templates')">
            <app-icon name="file-text" size="sm" /> Template Gallery
          </app-button>
          <app-button variant="primary" (btnClick)="navigateTo('/jobs')">
            <app-icon name="search" size="sm" /> Explore Jobs
          </app-button>
        </div>
      </app-page-header>

      @if (isLoading()) {
        <div class="loading-wrapper">
          <app-spinner size="lg" message="Loading executive metrics & pipeline feed..." />
        </div>
      } @else if (overview(); as data) {
        <!-- KPI Metrics Grid -->
        <div class="metrics-grid">
          <app-card class="metric-card">
            <div class="metric-header">
              <span class="metric-label">Total Applications</span>
              <app-icon name="file-text" size="md" class="metric-icon primary" />
            </div>
            <div class="metric-value">{{ data.metrics.totalApplied }}</div>
            <div class="metric-footer success">
              <app-icon name="arrow-right" size="xs" /> 8 active in pipeline
            </div>
          </app-card>

          <app-card class="metric-card">
            <div class="metric-header">
              <span class="metric-label">Active Interviews</span>
              <app-icon name="calendar" size="md" class="metric-icon warning" />
            </div>
            <div class="metric-value">{{ data.metrics.activeInterviews }}</div>
            <div class="metric-footer info">
              <app-icon name="check-circle" size="xs" /> Next in 2 days
            </div>
          </app-card>

          <app-card class="metric-card">
            <div class="metric-header">
              <span class="metric-label">Job Offers</span>
              <app-icon name="sparkles" size="md" class="metric-icon success" />
            </div>
            <div class="metric-value">{{ data.metrics.totalOffers }}</div>
            <div class="metric-footer success">
              <app-icon name="check-circle" size="xs" /> Ready for comparison
            </div>
          </app-card>

          <app-card class="metric-card">
            <div class="metric-header">
              <span class="metric-label">Response Rate</span>
              <app-icon name="grid" size="md" class="metric-icon primary" />
            </div>
            <div class="metric-value">{{ data.metrics.responseRatePercentage }}%</div>
            <div class="metric-footer success">
              <app-icon name="arrow-right" size="xs" /> Top 5% candidate pool
            </div>
          </app-card>
        </div>

        <!-- Upcoming Interview Banner -->
        @if (data.upcomingInterviews.length > 0; as interview) {
          <div class="upcoming-banner-card">
            <div class="banner-content">
              <div class="banner-badge">
                <app-badge variant="warning" styleMode="solid">
                  <app-icon name="calendar" size="xs" /> Upcoming Interview
                </app-badge>
              </div>
              <h3 class="banner-title">{{ data.upcomingInterviews[0].roundName }} — {{ data.upcomingInterviews[0].companyName }}</h3>
              <p class="banner-sub">
                Position: <strong>{{ data.upcomingInterviews[0].jobTitle }}</strong> • Scheduled with {{ data.upcomingInterviews[0].interviewerName }}
              </p>
            </div>
            <div class="banner-action">
              <app-button variant="primary" (btnClick)="navigateTo('/jobs')">
                View Application Details
              </app-button>
            </div>
          </div>
        }

        <!-- Two Column Main Body -->
        <div class="dashboard-body">
          <!-- Left Column: Activity Feed & Resume Trends -->
          <div class="main-column">
            <app-card title="Activity Feed & Telemetry">
              <div class="activity-list">
                @for (act of data.activityFeed; track act.id) {
                  <div class="activity-item">
                    <div class="activity-icon-badge">
                      <app-icon name="check-circle" size="sm" />
                    </div>
                    <div class="activity-details">
                      <h4 class="activity-title">{{ act.title }}</h4>
                      <p class="activity-desc">{{ act.description }}</p>
                      <span class="activity-time">{{ act.timestamp | date:'shortTime' }}</span>
                    </div>
                  </div>
                }
              </div>
            </app-card>

            <app-card title="Resume Score & ATS Readiness Trend" class="score-card">
              <div class="trend-wrapper">
                <div class="trend-score-row">
                  <span class="trend-score-value">{{ data.metrics.averageResumeScore }} / 100</span>
                  <app-badge variant="success">ATS Compatible</app-badge>
                </div>
                <app-progress-bar [value]="data.metrics.averageResumeScore" variant="success" />
                <p class="trend-caption">Resume matches high-value keywords for C# .NET and Angular architecture roles.</p>
              </div>
            </app-card>
          </div>

          <!-- Right Column: Quick Navigation & Notifications -->
          <div class="side-column">
            <app-card title="Quick Actions">
              <div class="quick-stack">
                <app-button variant="outline" [fullWidth]="true" (btnClick)="navigateTo('/jobs')">
                  <app-icon name="search" size="sm" /> Search Aggregated Jobs
                </app-button>
                <app-button variant="outline" [fullWidth]="true" (btnClick)="navigateTo('/resumes/templates')">
                  <app-icon name="file-text" size="sm" /> Resume Templates
                </app-button>
                <app-button variant="outline" [fullWidth]="true" (btnClick)="navigateTo('/resumes/import')">
                  <app-icon name="upload" size="sm" /> Import Resume Document
                </app-button>
                <app-button variant="secondary" [fullWidth]="true" (btnClick)="navigateTo('/design-system')">
                  <app-icon name="sparkles" size="sm" /> Fluent 2 Design System
                </app-button>
              </div>
            </app-card>

            <app-card title="Recent System Notifications">
              <div class="notification-stack">
                @for (notif of data.recentNotifications; track notif.id) {
                  <div class="notif-item" [class.unread]="!notif.isRead">
                    <div class="notif-header">
                      <span class="notif-title">{{ notif.title }}</span>
                      <app-badge variant="primary" styleMode="soft">{{ notif.type }}</app-badge>
                    </div>
                    <p class="notif-msg">{{ notif.message }}</p>
                  </div>
                }
              </div>
            </app-card>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: var(--space-6);
      max-width: 1400px;
      margin: 0 auto;
    }
    .dashboard-actions {
      display: flex;
      gap: var(--space-3);
    }
    .loading-wrapper {
      display: flex;
      justify-content: center;
      padding: var(--space-12) 0;
    }
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: var(--space-6);
      margin: var(--space-6) 0;
    }
    .metric-card {
      padding: var(--space-4);
    }
    .metric-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .metric-label {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      font-weight: 500;
    }
    .metric-icon {
      &.primary { color: var(--brand-primary); }
      &.warning { color: var(--warning); }
      &.success { color: var(--success); }
    }
    .metric-value {
      font-size: 32px;
      font-weight: 700;
      color: var(--text-primary);
      margin: var(--space-2) 0;
    }
    .metric-footer {
      font-size: var(--text-caption);
      display: flex;
      align-items: center;
      gap: var(--space-1);
      &.success { color: var(--success); }
      &.info { color: var(--brand-primary); }
    }
    .upcoming-banner-card {
      background: linear-gradient(135deg, var(--bg-elevated) 0%, var(--bg-tertiary) 100%);
      border: 1px solid var(--warning-border);
      border-radius: var(--radius-lg);
      padding: var(--space-6);
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-6);
      box-shadow: var(--shadow-sm);
    }
    .banner-title {
      font-size: var(--text-h3);
      font-weight: 700;
      margin: var(--space-2) 0 var(--space-1) 0;
    }
    .banner-sub {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      margin: 0;
    }
    .dashboard-body {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: var(--space-6);
      align-items: start;
    }
    @media (max-width: 992px) {
      .dashboard-body { grid-template-columns: 1fr; }
    }
    .main-column, .side-column {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    .activity-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .activity-item {
      display: flex;
      gap: var(--space-3);
      padding-bottom: var(--space-3);
      border-bottom: 1px solid var(--border-subtle);
    }
    .activity-icon-badge {
      color: var(--brand-primary);
      margin-top: 2px;
    }
    .activity-title {
      font-size: var(--text-body-sm);
      font-weight: 600;
      margin: 0 0 2px 0;
    }
    .activity-desc {
      font-size: var(--text-caption);
      color: var(--text-secondary);
      margin: 0 0 4px 0;
    }
    .activity-time {
      font-size: 11px;
      color: var(--text-muted);
    }
    .trend-score-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-2);
    }
    .trend-score-value {
      font-size: var(--text-h3);
      font-weight: 700;
    }
    .trend-caption {
      font-size: var(--text-caption);
      color: var(--text-secondary);
      margin: var(--space-2) 0 0 0;
    }
    .quick-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    .notification-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    .notif-item {
      padding: var(--space-3);
      border-radius: var(--radius-md);
      background-color: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      &.unread {
        border-color: var(--brand-primary);
        background-color: var(--brand-primary-alpha);
      }
    }
    .notif-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-1);
    }
    .notif-title {
      font-size: var(--text-body-sm);
      font-weight: 600;
    }
    .notif-msg {
      font-size: var(--text-caption);
      color: var(--text-secondary);
      margin: 0;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExecutiveDashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly router = inject(Router);

  protected readonly overview = this.dashboardService.overview;
  protected readonly isLoading = this.dashboardService.isLoading;

  ngOnInit(): void {
    this.dashboardService.loadOverview().subscribe();
  }

  protected navigateTo(url: string): void {
    this.router.navigateByUrl(url);
  }
}
