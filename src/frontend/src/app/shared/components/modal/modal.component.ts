import { Component, ChangeDetectionStrategy, input, output, HostListener } from '@angular/core';
import { FocusTrapDirective } from '../../directives/focus-trap.directive';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [FocusTrapDirective, ButtonComponent],
  template: `
    @if (isOpen()) {
      <div class="modal-backdrop" (click)="onBackdropClick()">
        <div
          class="modal-dialog"
          appFocusTrap
          role="dialog"
          aria-modal="true"
          [attr.aria-labelledby]="titleId"
          (click)="$event.stopPropagation()"
        >
          <div class="modal-header">
            <h2 [id]="titleId" class="modal-title">{{ title() }}</h2>
            <app-button variant="ghost" size="sm" (btnClick)="close()" aria-label="Close dialog">
              ✕
            </app-button>
          </div>
          <div class="modal-body">
            <ng-content />
          </div>
          <div class="modal-footer">
            <ng-content select="[modal-footer]" />
          </div>
        </div>
      </div>
    }
  `,
  styleUrl: './modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalComponent {
  readonly isOpen = input<boolean>(false);
  readonly title = input.required<string>();
  readonly titleId = 'modal-title-' + Math.random().toString(36).substring(2, 7);

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
