import { Injectable, inject } from '@angular/core';
import { NotificationService } from './notification.service';
import { environment } from '../../../environments/environment';
import { ProblemDetails } from '../models/problem-details.model';

@Injectable({
  providedIn: 'root',
})
export class ErrorService {
  private readonly notificationService = inject(NotificationService);

  handleError(error: unknown): void {
    if (environment.logging.enabled) {
      console.error('[ErrorService Caught Exception]:', error);
    }

    const message = this.extractErrorMessage(error);
    this.notificationService.error('Application Error', message);
  }

  handleHttpError(status: number, problemDetails?: ProblemDetails | null): void {
    let title = 'Network Error';
    let message = 'An unexpected server response occurred.';

    if (problemDetails?.title) {
      title = problemDetails.title;
    }
    if (problemDetails?.detail) {
      message = problemDetails.detail;
    } else {
      switch (status) {
        case 400:
          message = 'Bad Request. Please check your input parameters.';
          break;
        case 401:
          message = 'Unauthorized. Session expired or login required.';
          break;
        case 403:
          message = 'Access Denied. You do not have permission for this resource.';
          break;
        case 404:
          message = 'Resource not found.';
          break;
        case 500:
        case 502:
        case 503:
          message = 'Internal server error. Please try again later.';
          break;
      }
    }

    this.notificationService.error(title, message);
  }

  private extractErrorMessage(error: unknown): string {
    if (error instanceof Error) {
      return error.message;
    }
    if (typeof error === 'string') {
      return error;
    }
    return 'An unknown error occurred.';
  }
}
