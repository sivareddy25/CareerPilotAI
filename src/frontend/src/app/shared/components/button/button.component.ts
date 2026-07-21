import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { ElementVariant, ElementSize } from '../../types/component.types';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [],
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || loading()"
      [class]="buttonClasses()"
      (click)="onClick($event)"
      [attr.aria-busy]="loading()"
    >
      @if (loading()) {
        <span class="spinner" aria-hidden="true"></span>
      }
      <ng-content />
    </button>
  `,
  styleUrl: './button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ButtonComponent {
  readonly variant = input<ElementVariant>('primary');
  readonly size = input<ElementSize>('md');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly disabled = input<boolean>(false);
  readonly loading = input<boolean>(false);

  readonly btnClick = output<MouseEvent>();

  protected buttonClasses(): string {
    return `btn btn-${this.variant()} btn-${this.size()} ${this.loading() ? 'btn-loading' : ''}`;
  }

  protected onClick(event: MouseEvent): void {
    if (!this.disabled() && !this.loading()) {
      this.btnClick.emit(event);
    }
  }
}
