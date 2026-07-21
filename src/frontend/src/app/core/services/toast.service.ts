import { Injectable, inject } from '@angular/core';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly notificationService = inject(NotificationService);

  showSuccess(message: string, title = 'Success'): void {
    this.notificationService.success(title, message);
  }

  showError(message: string, title = 'Error'): void {
    this.notificationService.error(title, message);
  }

  showWarning(message: string, title = 'Warning'): void {
    this.notificationService.warning(title, message);
  }

  showInfo(message: string, title = 'Information'): void {
    this.notificationService.info(title, message);
  }
}
