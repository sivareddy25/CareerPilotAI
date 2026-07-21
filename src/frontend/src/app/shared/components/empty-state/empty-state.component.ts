import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, IconComponent, ButtonComponent],
  template: `
    <div class="empty-state-container">
      <div class="empty-icon-wrap">
        <app-icon [name]="icon()" size="xl" />
      </div>
      <h3 class="empty-title">{{ title() }}</h3>
      <p class="empty-desc">{{ description() }}</p>
      @if (actionLabel()) {
        <app-button variant="primary" (btnClick)="actionClick.emit()">
          {{ actionLabel() }}
        </app-button>
      }
    </div>
  `,
  styles: [`
    .empty-state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--space-12) var(--space-6);
      text-align: center;
      background-color: var(--bg-secondary);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      margin: var(--space-4) 0;
    }
    .empty-icon-wrap {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background-color: var(--brand-primary-alpha);
      color: var(--brand-primary);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: var(--space-4);
    }
    .empty-title {
      font-size: var(--text-h4);
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 var(--space-2) 0;
    }
    .empty-desc {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      max-width: 440px;
      margin: 0 0 var(--space-6) 0;
      line-height: 1.5;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly icon = input<IconName>('search');
  readonly actionLabel = input<string>('');

  readonly actionClick = output<void>();
}
