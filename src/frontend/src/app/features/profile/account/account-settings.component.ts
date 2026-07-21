import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { InputComponent } from '../../../shared/components/input/input.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import { toProfileErrorMessage } from '../profile-error';

/** Exact phrase the server requires; compared case-sensitively on both sides. */
const DELETE_CONFIRMATION = 'DELETE';

/**
 * Destructive account actions.
 *
 * Both are placed behind a password and behind an explicit reveal, and the delete
 * action additionally requires typing a confirmation phrase. The friction is the
 * feature: these are the only two actions in the product a user cannot undo themselves.
 */
@Component({
  selector: 'app-account-settings',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonComponent, CardComponent, InputComponent],
  template: `
    @if (errorMessage(); as message) {
      <p class="form-alert" role="alert" aria-live="assertive">{{ message }}</p>
    }

    <app-card title="Account status" subtitle="Your account is active.">
      <p class="explainer">
        Deactivating hides your profile and signs you out everywhere. An administrator
        can reverse it — your data is kept.
      </p>

      @if (!showDeactivate()) {
        <app-button variant="secondary" (btnClick)="showDeactivate.set(true)">
          Deactivate account
        </app-button>
      } @else {
        <form [formGroup]="deactivateForm" (ngSubmit)="deactivate()" novalidate>
          <div class="confirm-stack">
            <app-input
              label="Confirm your password"
              type="password"
              [required]="true"
              [value]="deactivateForm.controls.currentPassword.value"
              (valueChange)="deactivateForm.controls.currentPassword.setValue($event)"
            />
            <div class="button-row">
              <app-button type="submit" variant="danger" [loading]="deactivating()">
                Deactivate my account
              </app-button>
              <app-button variant="ghost" (btnClick)="showDeactivate.set(false)">Cancel</app-button>
            </div>
          </div>
        </form>
      }
    </app-card>

    <app-card title="Delete account" variant="outlined">
      <p class="explainer danger-text">
        Deleting removes your profile and signs you out permanently. This cannot be
        undone from the app.
      </p>

      @if (!showDelete()) {
        <app-button variant="danger" (btnClick)="showDelete.set(true)">
          Delete account
        </app-button>
      } @else {
        <form [formGroup]="deleteForm" (ngSubmit)="deleteAccount()" novalidate>
          <div class="confirm-stack">
            <app-input
              label="Confirm your password"
              type="password"
              [required]="true"
              [value]="deleteValue('currentPassword')"
              (valueChange)="setDelete('currentPassword', $event)"
            />
            <app-input
              [label]="'Type ' + confirmationPhrase + ' to confirm'"
              [required]="true"
              [value]="deleteValue('confirmation')"
              [error]="confirmationError()"
              (valueChange)="setDelete('confirmation', $event)"
            />
            <div class="button-row">
              <app-button
                type="submit"
                variant="danger"
                [loading]="deleting()"
                [disabled]="deleteForm.invalid"
              >
                Permanently delete my account
              </app-button>
              <app-button variant="ghost" (btnClick)="showDelete.set(false)">Cancel</app-button>
            </div>
          </div>
        </form>
      }
    </app-card>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; gap: var(--space-4); }
    .explainer {
      margin: 0 0 var(--space-4) 0;
      color: var(--text-secondary);
      font-size: var(--text-body-sm);
      max-width: 60ch;
    }
    .danger-text { color: var(--danger); }
    .confirm-stack { display: flex; flex-direction: column; gap: var(--space-3); max-width: 420px; }
    .button-row { display: flex; gap: var(--space-2); flex-wrap: wrap; }
    .form-alert {
      margin: 0;
      padding: var(--space-3);
      border: 1px solid var(--danger);
      border-radius: var(--radius-md);
      color: var(--danger);
      font-size: var(--text-body-sm);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AccountSettingsComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly profiles = inject(ProfileService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly confirmationPhrase = DELETE_CONFIRMATION;

  protected readonly showDeactivate = signal(false);
  protected readonly showDelete = signal(false);
  protected readonly deactivating = signal(false);
  protected readonly deleting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly deactivateForm = this.formBuilder.nonNullable.group({
    currentPassword: ['', [Validators.required]],
  });

  protected readonly deleteForm = this.formBuilder.nonNullable.group({
    currentPassword: ['', [Validators.required]],
    // Mirrors the server's check so the destructive button stays disabled until the
    // phrase is exact. The server repeats it — this is a guard rail, not the control.
    confirmation: ['', [Validators.required, Validators.pattern(`^${DELETE_CONFIRMATION}$`)]],
  });

  private readonly deleteValues = toSignal(this.deleteForm.valueChanges, {
    initialValue: this.deleteForm.getRawValue(),
  });

  protected deleteValue(control: 'currentPassword' | 'confirmation'): string {
    return this.deleteValues()[control] ?? '';
  }

  protected setDelete(control: string, value: string | number): void {
    this.deleteForm.get(control)?.setValue(String(value));
    this.deleteForm.get(control)?.markAsDirty();
  }

  protected confirmationError(): string | undefined {
    const control = this.deleteForm.controls.confirmation;
    return control.invalid && (control.dirty || control.touched)
      ? `Type ${DELETE_CONFIRMATION} exactly to confirm.`
      : undefined;
  }

  protected deactivate(): void {
    if (this.deactivateForm.invalid || this.deactivating()) {
      this.deactivateForm.markAllAsTouched();
      return;
    }

    this.deactivating.set(true);
    this.errorMessage.set(null);

    this.profiles.deactivate(this.deactivateForm.getRawValue()).subscribe({
      next: () => {
        this.deactivating.set(false);
        this.notifications.info('Account deactivated', 'Contact support to restore it.');
        void this.router.navigate(['/auth/login']);
      },
      error: (error: unknown) => {
        this.deactivating.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        this.deactivateForm.reset();
      },
    });
  }

  protected deleteAccount(): void {
    if (this.deleteForm.invalid || this.deleting()) {
      this.deleteForm.markAllAsTouched();
      return;
    }

    this.deleting.set(true);
    this.errorMessage.set(null);

    this.profiles.deleteAccount(this.deleteForm.getRawValue()).subscribe({
      next: () => {
        this.deleting.set(false);
        this.notifications.info('Account deleted');
        void this.router.navigate(['/auth/login']);
      },
      error: (error: unknown) => {
        this.deleting.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        this.deleteForm.reset();
      },
    });
  }
}
