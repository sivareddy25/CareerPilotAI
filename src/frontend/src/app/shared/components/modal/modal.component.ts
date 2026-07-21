import { Component, ChangeDetectionStrategy, input, output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FocusTrapDirective } from '../../directives/focus-trap.directive';
import { IconButtonComponent } from '../icon-button/icon-button.component';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule, FocusTrapDirective, IconButtonComponent],
  template: `
    @if (isOpen()) {
      <div class="dialog-backdrop" (click)="onBackdropClick()">
        <div
          class="dialog-card"
          [class]="'dialog-size-' + size()"
          appFocusTrap
          role="dialog"
          aria-modal="true"
          [attr.aria-labelledby]="titleId"
          (click)="$event.stopPropagation()"
        >
          <div class="dialog-header">
            <h2 [id]="titleId" class="dialog-title">{{ title() }}</h2>
            <app-icon-button icon="x" size="sm" ariaLabel="Close dialog" (btnClick)="close()" />
          </div>
          <div class="dialog-body">
            <ng-content />
          </div>
          <div class="dialog-footer">
            <ng-content select="[dialog-footer]" />
          </div>
        </div>
      </div>
    }
  `,
  styleUrl: './modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogComponent {
  readonly isOpen = input<boolean>(false);
  readonly title = input.required<string>();
  readonly size = input<'sm' | 'md' | 'lg' | 'xl'>('md');
  readonly titleId = 'dialog-title-' + Math.random().toString(36).substring(2, 7);

  readonly closed = output<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }

  protected close(): void {
    this.closed.emit();
  }

  protected onBackdropClick(): void {
    this.close();
  }
}

// Alias for backwards compatibility
export { DialogComponent as ModalComponent };
