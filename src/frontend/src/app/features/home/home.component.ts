import { Component, ChangeDetectionStrategy } from '@angular/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [PageHeaderComponent, CardComponent, ButtonComponent],
  template: `
    <app-page-header
      title="Welcome"
      subtitle="Production-ready Angular application infrastructure."
    >
      <app-button variant="primary" size="md">System Ready</app-button>
    </app-page-header>

    <div class="home-grid">
      <app-card title="Infrastructure Baseline" subtitle="Core framework initialization">
        <p>
          Angular application core services, layout shell, global signal state, SCSS variables, and HTTP interceptors are initialized.
        </p>
      </app-card>
      <app-card title="Feature Architecture" subtitle="Modular standalone setup">
        <p>
          Routing, functional guards, global error handling, and reusable UI components are configured for future features.
        </p>
      </app-card>
    </div>
  `,
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class HomeComponent {}
