import { HttpErrorResponse } from '@angular/common/http';

// Turns an HTTP error into a message fit to show the user. The API returns
// RFC 7807 problem details: business-rule failures put the reason in `detail`,
// model-binding failures list field messages under `errors`.
export function apiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'Could not reach the API. Make sure it is running.';
  }

  const body = error.error as { detail?: string; errors?: Record<string, string[]> } | null;

  if (body?.detail) {
    return body.detail;
  }

  if (body?.errors) {
    return Object.values(body.errors).flat().join(' ');
  }

  return fallback;
}
