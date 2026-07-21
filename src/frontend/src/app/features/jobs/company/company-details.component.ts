import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobService } from '../../../core/services/job.service';
import {
  CardComponent,
  PageHeaderComponent,
  ButtonComponent,
  IconComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-company-details',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    PageHeaderComponent,
    ButtonComponent,
    IconComponent,
  ],
  template: `
    <div class="company-container">
      <app-page-header
        title="Hiring Companies"
        subtitle="Companies actively ingesting jobs into CareerPilot AI."
      />

      <div class="companies-grid">
        @for (comp of companies(); track comp.id) {
          <app-card [title]="comp.name" [subtitle]="comp.industry || 'Tech / Enterprise'">
            <p class="company-desc">{{ comp.description || 'Hiring company posting open roles.' }}</p>
            <div card-footer class="company-actions">
              @if (comp.websiteUrl) {
                <a [href]="comp.websiteUrl" target="_blank" rel="noopener noreferrer" class="link">
                  <app-button variant="outline" size="sm">
                    <app-icon name="building" size="xs" /> Website
                  </app-button>
                </a>
              }
            </div>
          </app-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .company-container {
      padding: var(--space-6);
      max-width: 1200px;
      margin: 0 auto;
    }
    .companies-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: var(--space-6);
      margin-top: var(--space-6);
    }
    .company-desc {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      min-height: 48px;
    }
    .company-actions {
      display: flex;
      justify-content: flex-end;
      padding-top: var(--space-3);
    }
    .link { text-decoration: none; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompanyDetailsComponent implements OnInit {
  private readonly jobService = inject(JobService);

  protected readonly companies = this.jobService.companies;

  ngOnInit(): void {
    this.jobService.loadCompanies().subscribe();
  }
}
