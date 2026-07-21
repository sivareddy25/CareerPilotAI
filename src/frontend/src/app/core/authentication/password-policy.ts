import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Client-side mirror of the server's `Authentication` password policy.
 *
 * This exists purely so the user gets immediate feedback instead of a round-trip per
 * mistake. It is **not** an enforcement point — the server revalidates every field
 * with FluentValidation, and anything typed here can be bypassed by posting directly
 * to the API. When the two disagree, the server wins and its message is displayed.
 *
 * Keep these constants aligned with `AuthenticationOptions` in the backend.
 */
export const PASSWORD_POLICY = {
  minimumLength: 12,
  // BCrypt ignores input past 72 bytes; the server rejects rather than truncating.
  maximumLength: 72,
} as const;

export const PASSWORD_POLICY_HINT =
  'At least 12 characters, including an uppercase letter, a lowercase letter, a digit and a symbol.';

export function passwordPolicyValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string | null;

    if (!value) {
      return null; // Emptiness is `required`'s job, not this validator's.
    }

    const failures: string[] = [];

    if (value.length < PASSWORD_POLICY.minimumLength) {
      failures.push(`at least ${PASSWORD_POLICY.minimumLength} characters`);
    }

    if (value.length > PASSWORD_POLICY.maximumLength) {
      failures.push(`at most ${PASSWORD_POLICY.maximumLength} characters`);
    }

    if (!/[A-Z]/.test(value)) failures.push('an uppercase letter');
    if (!/[a-z]/.test(value)) failures.push('a lowercase letter');
    if (!/[0-9]/.test(value)) failures.push('a digit');
    if (!/[^a-zA-Z0-9]/.test(value)) failures.push('a symbol');

    return failures.length > 0 ? { passwordPolicy: { requirements: failures } } : null;
  };
}

/** Cross-field check for the confirm-password box. */
export function passwordsMatchValidator(
  passwordKey: string,
  confirmKey: string,
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordKey)?.value as string | null;
    const confirm = group.get(confirmKey)?.value as string | null;

    if (!password || !confirm || password === confirm) {
      return null;
    }

    return { passwordsMismatch: true };
  };
}
