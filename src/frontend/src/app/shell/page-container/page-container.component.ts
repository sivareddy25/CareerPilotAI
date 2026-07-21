import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-page-container',
  standalone: true,
  template: `
    <main id="main-content" class="page-container" tabindex="-1">
      <ng-content />
    </main>
  `,
  styleUrl: './page-container.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageContainerComponent {}
