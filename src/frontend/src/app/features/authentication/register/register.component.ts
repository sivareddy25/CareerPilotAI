import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { AuthenticationService } from '../../../core/authentication/authentication.service';
import { APP_CONSTANTS } from '../../../core/constants/app.constants';
import {
  PASSWORD_POLICY,
  PASSWORD_POLICY_HINT,
  passwordPolicyValidator,
  passwordsMatchValidator,
} from '../../../core/authentication/password-policy';
import { toAuthErrorMessage, toFieldErrors } from '../auth-error';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthenticationService);
  private readonly router = inject(Router);

  protected readonly passwordHint = PASSWORD_POLICY_HINT;
  protected readonly maxPasswordLength = PASSWORD_POLICY.maximumLength;

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fieldErrors = signal<Record<string, string>>({});

  protected readonly form = this.formBuilder.nonNullable.group(
    {
      firstName: ['', [Validators.maxLength(100)]],
      lastName: ['', [Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ['', [Validators.required, passwordPolicyValidator()]],
      confirmPassword: ['', [Validators.required]],
    },
    // Group-level, because it compares two controls. Never sent to the server: the
    // confirmation box is a typo guard for the user, not an input the API needs.
    { validators: passwordsMatchValidator('password', 'confirmPassword') },
  );

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    const { firstName, lastName, email, password } = this.form.getRawValue();

    this.auth
      .register({
        email,
        password,
        firstName: firstName.trim() || null,
        lastName: lastName.trim() || null,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          // Registration signs the user in, so there is no returnUrl to honour here.
          void this.router.navigateByUrl(APP_CONSTANTS.ROUTING.HOME);
        },
        error: (error: unknown) => {
          this.submitting.set(false);
          this.errorMessage.set(toAuthErrorMessage(error));
          this.fieldErrors.set(toFieldErrors(error));

          this.form.controls.password.reset();
          this.form.controls.confirmPassword.reset();
        },
      });
  }

  protected showError(control: 'firstName' | 'lastName' | 'email' | 'password'): boolean {
    const field = this.form.controls[control];
    return field.invalid && (field.dirty || field.touched);
  }

  protected get passwordRequirements(): string[] {
    const errors = this.form.controls.password.errors;
    return (errors?.['passwordPolicy']?.requirements as string[] | undefined) ?? [];
  }

  protected get passwordsMismatch(): boolean {
    const confirm = this.form.controls.confirmPassword;
    return this.form.hasError('passwordsMismatch') && (confirm.dirty || confirm.touched);
  }
}
