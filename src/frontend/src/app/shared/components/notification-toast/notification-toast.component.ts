import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [ButtonComponent],
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      @for (toast of notificationService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.type" role="alert">
          <div class="toast-content">
            <strong class="toast-title">{{ toast.title }}</strong>
            @if (toast.message) {
              <p class="toast-message">{{ toast.message }}</p>
            }
          </div>
          <app-button
            variant="ghost"
            size="sm"
            (btnClick)="notificationService.dismiss(toast.id)"
            aria-label="Dismiss notification"
          >
            ✕
          </app-button>
        </div>
      }
    </div>
  `,
  styleUrl: './notification-toast.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationToastComponent {
  protected readonly notificationService = inject(NotificationService);
}
