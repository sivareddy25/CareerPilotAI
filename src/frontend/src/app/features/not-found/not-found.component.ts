import { Component, ChangeDetectionStrategy } from '@angular/core';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [EmptyStateComponent, ButtonComponent, RouterLink],
  template: `
    <div class="not-found-container">
      <app-empty-state
        title="404 - Page Not Found"
        description="The requested page could not be found."
      >
        <div actions>
          <a routerLink="/">
            <app-button variant="primary" size="md">Return Home</app-button>
          </a>
        </div>
      </app-empty-state>
    </div>
  `,
  styleUrl: './not-found.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class NotFoundComponent {}
