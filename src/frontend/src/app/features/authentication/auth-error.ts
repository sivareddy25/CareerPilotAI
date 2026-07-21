import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../../core/models/problem-details.model';

/**
 * Turns an API failure into a message safe to show the user.
 *
 * The server writes RFC 7807 responses whose `detail` is deliberately worded for a
 * client to read — a wrong password says only that the credentials were incorrect,
 * never which half was wrong. So the detail is surfaced as-is where present, and only
 * the fallbacks are written here.
 */
export function toAuthErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  // A dropped connection or a CORS rejection. There is no response to read.
  if (error.status === 0) {
    return 'Cannot reach the server. Check your connection and try again.';
  }

  if (error.status === 429) {
    const retryAfter = error.headers?.get('Retry-After');
    return retryAfter
      ? `Too many attempts. Try again in ${retryAfter} seconds.`
      : 'Too many attempts. Please wait a moment and try again.';
  }

  const problem = (error.error ?? null) as ProblemDetails | null;

  // 400 carries per-field errors; those are bound to their controls by the caller, so
  // the banner only needs a summary.
  if (error.status === 400 && problem?.errors) {
    return 'Please correct the highlighted fields.';
  }

  if (problem?.detail) {
    return problem.detail;
  }

  return 'Something went wrong. Please try again.';
}

/**
 * Extracts the `errors` map from a 400 so each message can be attached to its control.
 *
 * Keys arrive as command property names (`Email`, `Password`) and are lower-cased to
 * match the form control names.
 */
export function toFieldErrors(error: unknown): Record<string, string> {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) {
    return {};
  }

  const problem = (error.error ?? null) as ProblemDetails | null;

  if (!problem?.errors) {
    return {};
  }

  const fieldErrors: Record<string, string> = {};

  for (const [key, messages] of Object.entries(problem.errors)) {
    const control = key.charAt(0).toLowerCase() + key.slice(1);
    fieldErrors[control] = messages.join(' ');
  }

  return fieldErrors;
}
