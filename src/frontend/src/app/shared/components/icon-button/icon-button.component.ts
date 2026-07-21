import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName, IconSize } from '../icon/icon.component';

@Component({
  selector: 'app-icon-button',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <button
      type="button"
      [disabled]="disabled()"
      [attr.aria-label]="ariaLabel()"
      [attr.title]="title() || ariaLabel()"
      [class]="buttonClasses()"
      (click)="onClick($event)"
    >
      <app-icon [name]="icon()" [size]="iconSize()" />
    </button>
  `,
  styles: [`
    .icon-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 0;
      border-radius: var(--radius-md);
      border: 1px solid transparent;
      background-color: transparent;
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover:not(:disabled) {
        background-color: var(--bg-tertiary);
        color: var(--text-primary);
      }

      &:active:not(:disabled) {
        transform: scale(0.95);
      }

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }

      &-sm { width: 32px; height: 32px; }
      &-md { width: 40px; height: 40px; }
      &-lg { width: 48px; height: 48px; }

      &-primary {
        background-color: var(--brand-primary-alpha);
        color: var(--brand-primary);
        &:hover:not(:disabled) {
          background-color: var(--brand-primary);
          color: var(--text-inverse);
        }
      }

      &-outline {
        border-color: var(--border-color);
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconButtonComponent {
  readonly icon = input.required<IconName>();
  readonly ariaLabel = input.required<string>();
  readonly title = input<string>('');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly variant = input<'ghost' | 'primary' | 'outline'>('ghost');
  readonly disabled = input<boolean>(false);

  readonly btnClick = output<MouseEvent>();

  protected iconSize = computed<IconSize>(() => {
    switch (this.size()) {
      case 'sm': return 'sm';
      case 'lg': return 'lg';
      default: return 'md';
    }
  });

  protected buttonClasses = computed(() => {
    return `icon-btn icon-btn-${this.size()} icon-btn-${this.variant()}`;
  });

  protected onClick(event: MouseEvent): void {
    if (!this.disabled()) {
      this.btnClick.emit(event);
    }
  }
}
