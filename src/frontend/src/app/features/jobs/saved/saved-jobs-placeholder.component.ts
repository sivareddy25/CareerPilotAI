import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  PageHeaderComponent,
  EmptyStateComponent,
  CardComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-saved-jobs-placeholder',
  standalone: true,
  imports: [
    CommonModule,
    PageHeaderComponent,
    EmptyStateComponent,
    CardComponent,
  ],
  template: `
    <div class="saved-container">
      <app-page-header
        title="Saved Jobs Placeholder"
        subtitle="Bookmark and track applications for saved opportunities."
      />

      <app-card class="saved-card">
        <app-empty-state
          title="No Saved Jobs Yet"
          description="Click the bookmark icon on any aggregated job posting to save it here for later tracking."
          icon="file-text"
        />
      </app-card>
    </div>
  `,
  styles: [`
    .saved-container {
      padding: var(--space-6);
      max-width: 800px;
      margin: 0 auto;
    }
    .saved-card {
      margin-top: var(--space-6);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedJobsPlaceholderComponent {}
