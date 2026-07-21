import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';
import { IconComponent, IconName } from '../icon/icon.component';
import { IconButtonComponent } from '../icon-button/icon-button.component';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule, IconComponent, IconButtonComponent],
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      @for (toast of notificationService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.type" role="alert">
          <app-icon [name]="getIcon(toast.type)" size="md" class="toast-icon" />
          <div class="toast-content">
            <strong class="toast-title">{{ toast.title }}</strong>
            @if (toast.message) {
              <p class="toast-message">{{ toast.message }}</p>
            }
          </div>
          <app-icon-button
            icon="x"
            size="sm"
            ariaLabel="Dismiss notification"
            (btnClick)="notificationService.dismiss(toast.id)"
          />
        </div>
      }
    </div>
  `,
  styleUrl: './notification-toast.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationToastComponent {
  protected readonly notificationService = inject(NotificationService);

  protected getIcon(type: string): IconName {
    switch (type) {
      case 'success': return 'check-circle';
      case 'error': return 'x-circle';
      case 'warning': return 'alert-triangle';
      default: return 'info';
    }
  }
}

export { NotificationToastComponent as SnackbarComponent };
