import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SkeletonType = 'text' | 'card' | 'avatar' | 'table' | 'rect';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="skeleton-box"
      [class]="'skeleton-' + type()"
      [style.width]="width()"
      [style.height]="height()"
      [style.border-radius]="borderRadius()"
    ></div>
  `,
  styles: [`
    .skeleton-box {
      display: block;
      background: linear-gradient(
        90deg,
        var(--bg-tertiary) 25%,
        var(--bg-secondary) 37%,
        var(--bg-tertiary) 63%
      );
      background-size: 400% 100%;
      animation: skeleton-shimmer 1.4s ease infinite;
      border-radius: var(--radius-md);
    }
    .skeleton-text {
      height: 16px;
      width: 100%;
      margin-bottom: 8px;
    }
    .skeleton-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }
    .skeleton-card {
      width: 100%;
      height: 160px;
      border-radius: var(--radius-lg);
    }
    .skeleton-table {
      width: 100%;
      height: 48px;
      border-radius: var(--radius-sm);
      margin-bottom: 8px;
    }

    @keyframes skeleton-shimmer {
      0% { background-position: 100% 50%; }
      100% { background-position: 0 50%; }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SkeletonComponent {
  readonly type = input<SkeletonType>('rect');
  readonly width = input<string>('100%');
  readonly height = input<string>('20px');
  readonly borderRadius = input<string>('var(--radius-md)');
}
