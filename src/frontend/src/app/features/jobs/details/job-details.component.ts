import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { JobService } from '../../../core/services/job.service';
import { AutomationService, UnansweredQuestionPrompt } from '../../../core/services/automation.service';
import { JobMatchExplanationDto } from '../../../core/models/job.models';
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
  MatchScoreComponent,
} from '../../../shared/components';
import { AnswerPromptModalComponent } from '../automation/answer-prompt-modal.component';

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
    MatchScoreComponent,
    AnswerPromptModalComponent,
  ],
  template: `
    <div class="details-container">
      <app-breadcrumb [items]="breadcrumbs" />

      @if (automationService.statusBanner(); as banner) {
        <div class="automation-banner" [ngClass]="banner.type">
          <app-icon [name]="banner.type === 'success' ? 'check-circle' : 'alert-triangle'" size="sm" />
          <span>{{ banner.message }}</span>
        </div>
      }

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

                <div class="action-buttons-header">
                  <app-button variant="primary" size="lg" [disabled]="automationService.isApplying()" (btnClick)="runAutoApply(j.id)">
                    <app-icon name="play" size="sm" /> {{ automationService.isApplying() ? 'Playwright Filling Form...' : 'Auto Apply with Playwright' }}
                  </app-button>

                  @if (j.applyUrl) {
                    <a [href]="j.applyUrl" target="_blank" rel="noopener noreferrer" class="apply-link">
                      <app-button variant="outline" size="lg">
                        <app-icon name="external-link" size="sm" /> Direct Site
                      </app-button>
                    </a>
                  }
                </div>
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
            <app-card title="AI Match Analysis" class="ai-match-card">
              @if (j.matchScore !== null && j.matchScore !== undefined) {
                <div class="match-score-badge">
                  <div class="score-number">{{ j.matchScore }}%</div>
                  <app-match-score [score]="j.matchScore" [summary]="j.matchSummary" [showLabel]="false" />
                </div>
                <app-progress-bar [value]="j.matchScore" [variant]="matchVariant(j.matchScore)" />

                @if (j.matchComponents && j.matchComponents.length > 0) {
                  <div class="component-list">
                    @for (c of j.matchComponents; track c.name) {
                      <div class="component-row" [title]="c.detail">
                        <span class="component-name">{{ c.name }}</span>
                        <span class="component-score">{{ c.score }}</span>
                      </div>
                    }
                  </div>
                }

                <app-button variant="primary" [fullWidth]="true" [disabled]="isExplaining()" (btnClick)="explainMatch()">
                  <app-icon name="sparkles" size="sm" />
                  {{ isExplaining() ? 'Analyzing with local AI…' : 'Why does this match?' }}
                </app-button>

                @if (explanation(); as ex) {
                  <div class="explanation-box">
                    @if (ex.strengths.length > 0) {
                      <h4><app-icon name="check" size="sm" /> Strengths</h4>
                      <ul>
                        @for (s of ex.strengths; track s) { <li>{{ s }}</li> }
                      </ul>
                    }
                    @if (ex.gaps.length > 0) {
                      <h4 class="gaps-title"><app-icon name="alert-triangle" size="sm" /> Gaps</h4>
                      <ul>
                        @for (g of ex.gaps; track g) { <li>{{ g }}</li> }
                      </ul>
                    }
                    <p class="recommendation">{{ ex.recommendation }}</p>
                    <span class="ai-source">{{ ex.generatedByAi ? 'Generated by local AI (Ollama)' : 'Computed from your profile' }}</span>
                  </div>
                }
              } @else {
                <div class="no-profile-hint">
                  <app-icon name="sparkles" size="sm" />
                  <p>Set up your <a (click)="navigateTo('/profile/career')">career profile</a> to see how well this job matches your skills.</p>
                </div>
              }

              <div class="action-buttons-stack">
                <app-button variant="outline" [fullWidth]="true" (btnClick)="navigateTo('/resumes/templates')">
                  <app-icon name="file-text" size="sm" /> Tailor Resume for Job
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

    <!-- Missing Answer Vault Prompt Modal -->
    <app-answer-prompt-modal
      [isOpen]="isModalOpen()"
      [questions]="missingQuestions()"
      (closed)="isModalOpen.set(false)"
      (answersSubmitted)="onAnswersSubmitted($event)"
    />
  `,
  styles: [`
    .details-container {
      padding: var(--space-6);
      max-width: 1300px;
      margin: 0 auto;
    }
    .automation-banner {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-3) var(--space-4);
      border-radius: var(--radius-md);
      margin-top: var(--space-4);
      font-size: var(--text-body-sm);
      font-weight: 500;
      &.success {
        background-color: rgba(22, 163, 74, 0.1);
        border: 1px solid rgba(22, 163, 74, 0.3);
        color: #16a34a;
      }
      &.warning {
        background-color: rgba(217, 119, 6, 0.1);
        border: 1px solid rgba(217, 119, 6, 0.3);
        color: #d97706;
      }
      &.error {
        background-color: rgba(220, 38, 38, 0.1);
        border: 1px solid rgba(220, 38, 38, 0.3);
        color: #dc2626;
      }
    }
    .action-buttons-header {
      display: flex;
      align-items: center;
      gap: var(--space-3);
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
    .action-buttons-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      margin-top: var(--space-4);
    }
    .component-list {
      margin: var(--space-4) 0;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    .component-row {
      display: flex;
      justify-content: space-between;
      font-size: var(--text-body-sm);
      padding: 2px 0;
      border-bottom: 1px dashed var(--border-subtle);
    }
    .component-name { color: var(--text-secondary); }
    .component-score { font-weight: 700; font-variant-numeric: tabular-nums; color: var(--text-primary); }
    .explanation-box {
      margin-top: var(--space-4);
      padding: var(--space-3);
      border-radius: var(--radius-md);
      background-color: var(--ai-accent-bg, var(--surface-muted));
      h4 {
        margin: var(--space-2) 0 var(--space-1) 0;
        font-size: var(--text-body-sm);
        display: flex; align-items: center; gap: var(--space-2);
        color: var(--color-success, #16a34a);
      }
      h4.gaps-title { color: var(--color-warning, #d97706); }
      ul { margin: 0; padding-left: var(--space-4); font-size: var(--text-caption); color: var(--text-secondary); }
    }
    .recommendation {
      margin: var(--space-3) 0 var(--space-2) 0;
      font-size: var(--text-body-sm);
      font-style: italic;
      color: var(--text-primary);
    }
    .ai-source { font-size: var(--text-caption); color: var(--text-muted); }
    .no-profile-hint {
      display: flex; gap: var(--space-2); align-items: flex-start;
      padding: var(--space-3);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      a { color: var(--brand-primary); cursor: pointer; text-decoration: underline; }
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
  protected readonly automationService = inject(AutomationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly job = this.jobService.activeJob;
  protected readonly isLoading = this.jobService.isLoading;

  protected readonly explanation = signal<JobMatchExplanationDto | null>(null);
  protected readonly isExplaining = signal(false);

  protected readonly isModalOpen = signal(false);
  protected readonly missingQuestions = signal<UnansweredQuestionPrompt[]>([]);
  private activeJobId: string = '';

  protected readonly breadcrumbs: BreadcrumbItem[] = [
    { label: 'Job Aggregator', url: '/jobs' },
    { label: 'Job Details', url: '' },
  ];

  ngOnInit(): void {
    this.automationService.statusBanner.set(null);
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.activeJobId = id;
      this.jobService.getJobById(id).subscribe();
    }
  }

  protected runAutoApply(jobId: string): void {
    this.automationService.executeAutoApply(jobId).subscribe((res) => {
      if (!res.success && res.missingQuestions && res.missingQuestions.length > 0) {
        this.missingQuestions.set(res.missingQuestions);
        this.isModalOpen.set(true);
      }
    });
  }

  protected onAnswersSubmitted(answers: Record<string, string>): void {
    const questions = this.missingQuestions();
    let savedCount = 0;
    questions.forEach((q) => {
      const val = answers[q.questionKey];
      if (val) {
        this.automationService.saveCandidateAnswer(q.questionKey, q.questionText, val).subscribe(() => {
          savedCount++;
          if (savedCount === questions.length) {
            this.isModalOpen.set(false);
            this.runAutoApply(this.activeJobId);
          }
        });
      }
    });
  }

  protected explainMatch(): void {
    const current = this.job();
    if (!current || this.isExplaining()) {
      return;
    }

    this.isExplaining.set(true);
    this.explanation.set(null);
    this.jobService.explainMatch(current.id).subscribe((result) => {
      this.explanation.set(result);
      this.isExplaining.set(false);
    });
  }

  protected matchVariant(score: number): 'success' | 'primary' | 'warning' {
    if (score >= 85) return 'success';
    if (score >= 70) return 'primary';
    return 'warning';
  }

  protected navigateTo(url: string): void {
    this.router.navigateByUrl(url);
  }
}
