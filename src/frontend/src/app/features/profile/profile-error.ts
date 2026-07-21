import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../../core/models/problem-details.model';

/**
 * Turns an API failure into a message safe to show the user.
 *
 * Mirrors the authentication module's helper deliberately rather than sharing it: the
 * two surfaces have different failure vocabularies, and merging them would mean a
 * change made for sign-in silently altering what a profile save reports.
 */
export function toProfileErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Check your connection and try again.';
  }

  if (error.status === 413) {
    return 'That file is too large. Choose an image under 2 MB.';
  }

  const problem = (error.error ?? null) as ProblemDetails | null;

  if (error.status === 400 && problem?.errors) {
    return 'Please correct the highlighted fields.';
  }

  if (problem?.detail) {
    return problem.detail;
  }

  return 'Something went wrong. Please try again.';
}

/**
 * Extracts a 400's per-field errors, keyed to match form control names.
 *
 * FluentValidation reports nested paths for owned types, so a key can arrive as
 * `Preferences.Theme`. Only the final segment is kept, lower-cased, because that is
 * what the control is called on the client.
 */
export function toProfileFieldErrors(error: unknown): Record<string, string> {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) {
    return {};
  }

  const problem = (error.error ?? null) as ProblemDetails | null;

  if (!problem?.errors) {
    return {};
  }

  const fieldErrors: Record<string, string> = {};

  for (const [key, messages] of Object.entries(problem.errors)) {
    const leaf = key.split('.').pop() ?? key;
    const control = leaf.charAt(0).toLowerCase() + leaf.slice(1);
    fieldErrors[control] = messages.join(' ');
  }

  return fieldErrors;
}
