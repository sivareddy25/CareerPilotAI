import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { AuthenticationService } from '../../../core/authentication/authentication.service';
import { APP_CONSTANTS } from '../../../core/constants/app.constants';
import { toAuthErrorMessage, toFieldErrors } from '../auth-error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthenticationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fieldErrors = signal<Record<string, string>>({});

  /**
   * Validation here is only shape — presence and a plausible email. Password
   * composition rules are not applied on sign-in: an account created before a policy
   * change must still be able to reach the screen where it can be updated.
   */
  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);

        // returnUrl is read from the query string, which is attacker-controllable, so
        // only same-origin relative paths are honoured. Without this check a crafted
        // link could bounce a freshly authenticated user to an external site.
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const target =
          returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//')
            ? returnUrl
            : APP_CONSTANTS.ROUTING.HOME;

        void this.router.navigateByUrl(target);
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(toAuthErrorMessage(error));
        this.fieldErrors.set(toFieldErrors(error));

        // The password box is cleared but the email is kept: retyping an address that
        // was already correct is friction with no security value.
        this.form.controls.password.reset();
      },
    });
  }

  protected showError(control: 'email' | 'password'): boolean {
    const field = this.form.controls[control];
    return field.invalid && (field.dirty || field.touched);
  }
}
