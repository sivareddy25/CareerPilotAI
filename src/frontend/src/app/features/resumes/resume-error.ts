import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../../core/models/problem-details.model';

/** Turns an API failure into a message safe to show the user. */
export function toResumeErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Check your connection and try again.';
  }

  // The server caps a whole import batch, so this is reached by uploading many large
  // files at once rather than one oversized file.
  if (error.status === 413) {
    return 'Those files are too large. Import fewer at a time, or use smaller files.';
  }

  if (error.status === 404) {
    return 'That resume no longer exists.';
  }

  const problem = (error.error ?? null) as ProblemDetails | null;

  if (error.status === 400 && problem?.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  if (problem?.detail) {
    return problem.detail;
  }

  return 'Something went wrong. Please try again.';
}

/**
 * Reads a ProblemDetails body out of a blob response.
 *
 * Export requests ask for `responseType: 'blob'`, so an error body arrives as a Blob
 * rather than parsed JSON and `error.error` is unreadable without this.
 */
export async function readBlobError(error: unknown): Promise<string> {
  if (error instanceof HttpErrorResponse && error.error instanceof Blob) {
    try {
      const text = await error.error.text();
      const problem = JSON.parse(text) as ProblemDetails;

      if (problem.detail) return problem.detail;
    } catch {
      // Not JSON, or unreadable. Fall through to the generic mapping.
    }
  }

  return toResumeErrorMessage(error);
}
