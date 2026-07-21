import { HttpInterceptorFn } from '@angular/common/http';

export const authenticationInterceptor: HttpInterceptorFn = (req, next) => {
  // Empty placeholder authentication interceptor as per specifications.
  // Bearer token injection logic will be placed here in future iterations.
  return next(req);
};
