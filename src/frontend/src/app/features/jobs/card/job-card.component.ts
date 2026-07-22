import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  ChipComponent,
  IconComponent,
  MatchScoreComponent,
} from '../../../shared/components';
import { JobDto, RemoteType } from '../../../core/models/job.models';

@Component({
  selector: 'app-job-card',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    ChipComponent,
    IconComponent,
    MatchScoreComponent,
  ],
  template: `
    <app-card [hoverable]="true" class="job-card-wrapper">
      <div class="job-card-header">
        <div class="company-meta">
          <div class="company-logo-placeholder">
            {{ job().company.name.charAt(0) }}
          </div>
          <div>
            <h4 class="company-name">{{ job().company.name }}</h4>
            <span class="source-badge">via {{ job().sourceName }}</span>
          </div>
        </div>

        <div class="header-badges">
          <app-match-score [score]="job().matchScore" [summary]="job().matchSummary" />
          <app-badge [variant]="getRemoteBadgeVariant(job().location.remoteType)">
            {{ job().location.displayLocation }}
          </app-badge>
        </div>
      </div>

      <h3 class="job-title">{{ job().title }}</h3>

      <div class="job-details-row">
        <span class="detail-item">
          <app-icon name="dollar-sign" size="xs" /> {{ job().salary.formattedRange }}
        </span>
      </div>

      <p class="job-description-snippet">
        {{ job().description }}
      </p>

      @if (job().matchedSkills && job().matchedSkills!.length > 0) {
        <div class="matched-skills">
          <app-icon name="check" size="xs" />
          <span>Your skills: {{ job().matchedSkills!.join(', ') }}</span>
        </div>
      }

      <div class="skills-chips">
        @for (skill of job().skills; track skill) {
          <app-chip>{{ skill }}</app-chip>
        }
      </div>

      <div card-footer class="job-card-footer">
        <span class="posted-date">Posted {{ job().postedAt | date:'mediumDate' }}</span>
        <app-button variant="primary" size="sm" (btnClick)="viewDetails.emit(job().id)">
          View Position
        </app-button>
      </div>
    </app-card>
  `,
  styles: [`
    .job-card-wrapper {
      height: 100%;
      display: flex;
      flex-direction: column;
    }
    .job-card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: var(--space-3);
    }
    .header-badges {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: var(--space-1);
    }
    .matched-skills {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      font-size: var(--text-caption);
      color: var(--color-success, #16a34a);
      margin: 0 0 var(--space-3) 0;
    }
    .company-meta {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }
    .company-logo-placeholder {
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background-color: var(--brand-primary-alpha);
      color: var(--brand-primary);
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: var(--text-h4);
    }
    .company-name {
      font-size: var(--text-body-sm);
      font-weight: 600;
      margin: 0;
      color: var(--text-primary);
    }
    .source-badge {
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
    .job-title {
      font-size: var(--text-h4);
      font-weight: 600;
      color: var(--text-primary);
      margin: 0 0 var(--space-2) 0;
      line-height: 1.3;
    }
    .job-details-row {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      font-size: var(--text-caption);
      color: var(--text-secondary);
      margin-bottom: var(--space-3);
    }
    .detail-item {
      display: flex;
      align-items: center;
      gap: var(--space-1);
    }
    .job-description-snippet {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      line-height: 1.5;
      margin: 0 0 var(--space-4) 0;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .skills-chips {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-1);
      margin-bottom: var(--space-4);
    }
    .job-card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: var(--space-3);
      border-top: 1px solid var(--border-subtle);
    }
    .posted-date {
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobCardComponent {
  readonly job = input.required<JobDto>();
  readonly viewDetails = output<string>();

  protected getRemoteBadgeVariant(remoteType: RemoteType): 'success' | 'warning' | 'secondary' {
    switch (remoteType) {
      case RemoteType.Remote: return 'success';
      case RemoteType.Hybrid: return 'warning';
      default: return 'secondary';
    }
  }
}
