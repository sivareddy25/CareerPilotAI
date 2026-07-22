import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent, IconComponent } from '../../../shared/components';

@Component({
  selector: 'app-search-criteria-banner',
  standalone: true,
  imports: [CommonModule, ButtonComponent, IconComponent],
  template: `
    <div class="search-banner-container">
      <div class="banner-left">
        <div class="agent-badge">
          <app-icon name="sparkles" size="sm" />
          <span>AI Recruiter Active</span>
        </div>
        <div class="banner-info">
          <span class="banner-title">Continuously searching worldwide jobs for:</span>
          <div class="criteria-pills">
            <span class="pill-bold">{{ targetTitles() || '.NET Full Stack Developer' }}</span>
            <span class="dot">•</span>
            <span>{{ yearsExp() || 5 }} Yrs Experience</span>
            <span class="dot">•</span>
            <span>{{ remoteType() || 'Remote / Hybrid' }}</span>
            <span class="dot">•</span>
            <span>{{ salary() || '$140,000 / year' }}</span>
            <span class="dot">•</span>
            <span>{{ skillsCount() || 10 }} Skills Matched</span>
          </div>
        </div>
      </div>

      <div class="banner-actions">
        <app-button variant="outline" size="sm" (btnClick)="editClick.emit()">
          <app-icon name="edit" size="xs" /> Edit Profile
        </app-button>
      </div>
    </div>
  `,
  styles: [`
    .search-banner-container {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4) var(--space-5);
      background: linear-gradient(135deg, rgba(79, 70, 229, 0.08) 0%, rgba(147, 51, 234, 0.08) 100%);
      border: 1px solid rgba(124, 58, 237, 0.2);
      border-radius: var(--radius-lg);
      margin-bottom: var(--space-6);
      gap: var(--space-4);
    }
    @media (max-width: 768px) {
      .search-banner-container {
        flex-direction: column;
        align-items: flex-start;
      }
    }
    .banner-left {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    .agent-badge {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-full);
      background-color: var(--brand-primary);
      color: #ffffff;
      font-size: var(--text-caption);
      font-weight: 600;
      white-space: nowrap;
    }
    .banner-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .banner-title {
      font-size: var(--text-caption);
      color: var(--text-secondary);
      font-weight: 500;
    }
    .criteria-pills {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: var(--space-2);
      font-size: var(--text-body-sm);
      color: var(--text-primary);
    }
    .pill-bold {
      font-weight: 700;
      color: var(--brand-primary);
    }
    .dot {
      color: var(--text-muted);
    }
    .banner-actions {
      display: flex;
      align-items: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchCriteriaBannerComponent {
  readonly targetTitles = input<string>('');
  readonly yearsExp = input<number>(5);
  readonly remoteType = input<string>('');
  readonly salary = input<string>('');
  readonly skillsCount = input<number>(10);
  readonly editClick = output<void>();
}
