import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  standalone: true,
  template: `
    <div class="loading-indicator" [class]="'size-' + size()" role="status" [attr.aria-label]="label()">
      <div class="spinner"></div>
      @if (label()) {
        <span class="loading-label">{{ label() }}</span>
      }
    </div>
  `,
  styleUrl: './loading-indicator.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingIndicatorComponent {
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly label = input<string>('Loading...');
}
