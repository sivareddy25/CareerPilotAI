import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AppConfigService } from '../config/app-config.service';

/**
 * Rewrites root-relative `/api/...` requests onto the configured backend origin.
 *
 * Several feature services address the API with paths like `/api/v1/jobs`. Under `ng serve`
 * those resolve against the dev server (`:4200`), which answers every unknown path with the SPA's
 * `index.html` — so the call "succeeds" with an HTML body, JSON parsing fails, and the service
 * silently falls back to its mock data. That is why real jobs, scores and dashboard figures never
 * appeared in the app despite the backend returning them.
 *
 * Prepending the API origin (scheme + host + port taken from `apiBaseUrl`) sends these to the real
 * backend, which CORS already permits from the dev origin. Absolute URLs and non-`/api` paths are
 * left untouched, so this cannot interfere with asset or third-party requests.
 */
export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/')) {
    return next(req);
  }

  const apiBaseUrl = inject(AppConfigService).apiBaseUrl;

  // apiBaseUrl is the full API root (e.g. http://localhost:5080/api/v1); only its origin is
  // needed here because the request path already carries the /api/v1 segment. Relative config
  // values (a same-origin deployment) yield no origin, in which case the request is left as-is
  // and resolves same-origin — correct for that setup.
  let origin = '';
  try {
    origin = new URL(apiBaseUrl, window.location.origin).origin;
  } catch {
    origin = '';
  }

  if (!origin || origin === window.location.origin) {
    return next(req);
  }

  return next(req.clone({ url: `${origin}${req.url}` }));
};
