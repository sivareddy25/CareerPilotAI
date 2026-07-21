import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';
import { LoadingIndicatorComponent } from '../loading-indicator/loading-indicator.component';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [LoadingIndicatorComponent],
  template: `
    @if (loadingService.isLoading()) {
      <div class="loading-overlay" role="dialog" aria-modal="true" aria-label="Loading content">
        <app-loading-indicator size="lg" label="Processing request..." />
      </div>
    }
  `,
  styleUrl: './loading-overlay.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingOverlayComponent {
  protected readonly loadingService = inject(LoadingService);
}
