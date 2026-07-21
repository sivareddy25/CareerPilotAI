import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SkeletonType = 'text' | 'circle' | 'rect';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [class]="skeletonClasses()"
      [style.width]="width()"
      [style.height]="height()"
      aria-hidden="true"
    ></div>
  `,
  styles: [`
    .skeleton {
      background: linear-gradient(
        90deg,
        var(--bg-tertiary) 25%,
        var(--border-subtle) 50%,
        var(--bg-tertiary) 75%
      );
      background-size: 200% 100%;
      animation: shimmer 1.5s infinite linear;
      border-radius: var(--radius-md);

      &-text {
        height: 1rem;
        margin-bottom: var(--space-2);
      }

      &-circle {
        border-radius: var(--radius-circle);
      }

      &-rect {
        height: 100px;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SkeletonComponent {
  readonly type = input<SkeletonType>('text');
  readonly width = input<string>('100%');
  readonly height = input<string>();

  protected skeletonClasses = computed(() => `skeleton skeleton-${this.type()}`);
}
