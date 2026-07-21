import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { ErrorService } from '../services/error.service';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/problem-details.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorService = inject(ErrorService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let problemDetails: ProblemDetails | null = null;
      if (error.error && typeof error.error === 'object') {
        problemDetails = error.error as ProblemDetails;
      }
      errorService.handleHttpError(error.status, problemDetails);
      return throwError(() => error);
    })
  );
};
