import { HttpInterceptorFn } from '@angular/common/http';
import { API_CONSTANTS } from '../constants/api.constants';
import { StringUtils } from '../../shared/utils/string.utils';

export const requestIdInterceptor: HttpInterceptorFn = (req, next) => {
  const requestId = StringUtils.generateUuid();
  const modifiedReq = req.clone({
    headers: req.headers.set(API_CONSTANTS.HEADERS.REQUEST_ID, requestId),
  });
  return next(modifiedReq);
};
