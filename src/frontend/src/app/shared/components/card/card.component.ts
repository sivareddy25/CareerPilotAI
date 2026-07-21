import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardVariant = 'elevated' | 'outlined' | 'filled' | 'glass';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cardClasses()">
      @if (title() || subtitle()) {
        <div class="card-header">
          <div class="card-header-text">
            @if (title()) {
              <h3 class="card-title">{{ title() }}</h3>
            }
            @if (subtitle()) {
              <p class="card-subtitle">{{ subtitle() }}</p>
            }
          </div>
          <div class="card-header-actions">
            <ng-content select="[card-header-actions]" />
          </div>
        </div>
      }
      <div class="card-body">
        <ng-content />
      </div>
      <div class="card-footer">
        <ng-content select="[card-footer]" />
      </div>
    </div>
  `,
  styleUrl: './card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  readonly title = input<string>();
  readonly subtitle = input<string>();
  readonly variant = input<CardVariant>('elevated');
  readonly hoverable = input<boolean>(false);
  readonly padding = input<'sm' | 'md' | 'lg' | 'none'>('md');

  protected cardClasses = computed(() => {
    return [
      'card',
      `card-${this.variant()}`,
      `card-padding-${this.padding()}`,
      this.hoverable() ? 'card-hoverable' : '',
    ].filter(Boolean).join(' ');
  });
}
