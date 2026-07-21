import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { InputComponent } from '../../../shared/components/input/input.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  PASSWORD_POLICY,
  PASSWORD_POLICY_HINT,
  passwordPolicyValidator,
  passwordsMatchValidator,
} from '../../../core/authentication/password-policy';
import { toProfileErrorMessage, toProfileFieldErrors } from '../profile-error';

/**
 * Password management.
 *
 * Reuses the authentication module's password policy rather than restating it, so the
 * rules a user is held to when changing a password are by construction the same ones
 * they were held to when registering.
 */
@Component({
  selector: 'app-security-settings',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonComponent, CardComponent, InputComponent],
  template: `
    <app-card title="Change password" subtitle="You will be signed out of all devices.">
      @if (errorMessage(); as message) {
        <p class="form-alert" role="alert" aria-live="assertive">{{ message }}</p>
      }

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <div class="stack">
          <app-input
            label="Current password"
            type="password"
            [required]="true"
            [value]="value('currentPassword')"
            [error]="errorFor('currentPassword')"
            (valueChange)="set('currentPassword', $event)"
          />
          <app-input
            label="New password"
            type="password"
            [required]="true"
            [helperText]="passwordHint"
            [value]="value('newPassword')"
            [error]="newPasswordError()"
            (valueChange)="set('newPassword', $event)"
          />
          <app-input
            label="Confirm new password"
            type="password"
            [required]="true"
            [value]="value('confirmPassword')"
            [error]="mismatch() ? 'Both passwords must match.' : undefined"
            (valueChange)="set('confirmPassword', $event)"
          />
        </div>

        <p class="signout-note">
          Changing your password revokes every active session, including this one, so
          you will need to sign in again.
        </p>

        <div class="form-actions">
          <app-button type="submit" variant="primary" [loading]="saving()">
            {{ saving() ? 'Updating…' : 'Update password' }}
          </app-button>
        </div>
      </form>
    </app-card>
  `,
  styles: [`
    .stack { display: flex; flex-direction: column; gap: var(--space-4); max-width: 420px; }
    .form-alert {
      margin: 0 0 var(--space-3) 0;
      padding: var(--space-3);
      border: 1px solid var(--danger);
      border-radius: var(--radius-md);
      color: var(--danger);
      font-size: var(--text-body-sm);
    }
    .signout-note {
      margin: var(--space-4) 0 0 0;
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      max-width: 420px;
    }
    .form-actions { margin-top: var(--space-4); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class SecuritySettingsComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly profiles = inject(ProfileService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly passwordHint = PASSWORD_POLICY_HINT;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fieldErrors = signal<Record<string, string>>({});

  protected readonly form = this.formBuilder.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: [
        '',
        [Validators.required, Validators.maxLength(PASSWORD_POLICY.maximumLength), passwordPolicyValidator()],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator('newPassword', 'confirmPassword') },
  );

  private readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  protected value(control: 'currentPassword' | 'newPassword' | 'confirmPassword'): string {
    return this.formValue()[control] ?? '';
  }

  protected set(control: string, next: string | number): void {
    this.form.get(control)?.setValue(String(next));
    this.form.get(control)?.markAsDirty();
  }

  protected errorFor(control: string): string | undefined {
    return this.fieldErrors()[control];
  }

  protected newPasswordError(): string | undefined {
    const control = this.form.controls.newPassword;
    const requirements = control.errors?.['passwordPolicy']?.requirements as string[] | undefined;

    if (requirements && (control.dirty || control.touched)) {
      return `Password needs ${requirements.join(', ')}.`;
    }

    return this.fieldErrors()['newPassword'];
  }

  protected mismatch(): boolean {
    const confirm = this.form.controls.confirmPassword;
    return this.form.hasError('passwordsMismatch') && (confirm.dirty || confirm.touched);
  }

  protected submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    const { currentPassword, newPassword } = this.form.getRawValue();

    this.profiles.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.saving.set(false);
        this.notifications.success('Password updated', 'Sign in again with your new password.');
        // The service has already cleared the local session, so this navigation lands
        // on the sign-in page rather than bouncing off the auth guard.
        void this.router.navigate(['/auth/login']);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        this.fieldErrors.set(toProfileFieldErrors(error));
        this.form.controls.currentPassword.reset();
      },
    });
  }
}
