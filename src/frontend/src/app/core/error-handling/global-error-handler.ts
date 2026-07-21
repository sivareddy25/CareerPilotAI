import { ErrorHandler, Injectable, inject, NgZone } from '@angular/core';
import { ErrorService } from '../services/error.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly errorService = inject(ErrorService);
  private readonly zone = inject(NgZone);

  handleError(error: unknown): void {
    // Run inside NgZone so notifications trigger change detection properly
    this.zone.run(() => {
      this.errorService.handleError(error);
    });
  }
}
