import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { InputComponent } from '../../../shared/components/input/input.component';
import { SelectComponent } from '../../../shared/components/select/select.component';
import { TextareaComponent } from '../../../shared/components/textarea/textarea.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import { countryOptions, detectedTimeZone, languageOptions, timeZoneOptions } from '../reference-data';
import { toProfileErrorMessage, toProfileFieldErrors } from '../profile-error';

/** Matches the server's `ProfileRules.BioMaxLength`. */
const BIO_MAX = 1000;

@Component({
  selector: 'app-profile-edit',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AvatarComponent,
    ButtonComponent,
    CardComponent,
    InputComponent,
    SelectComponent,
    TextareaComponent,
  ],
  templateUrl: './profile-edit.component.html',
  styles: [`
    :host { display: flex; flex-direction: column; gap: var(--space-4); }
    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: var(--space-4);
    }
    .form-grid.single { grid-template-columns: 1fr; }
    .picture-row {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      flex-wrap: wrap;
    }
    .picture-actions { display: flex; gap: var(--space-2); flex-wrap: wrap; }
    .visually-hidden {
      position: absolute;
      width: 1px; height: 1px;
      padding: 0; margin: -1px;
      overflow: hidden;
      clip: rect(0 0 0 0);
      white-space: nowrap;
      border: 0;
    }
    .form-alert {
      margin: 0 0 var(--space-3) 0;
      padding: var(--space-3);
      border: 1px solid var(--danger);
      border-radius: var(--radius-md);
      color: var(--danger);
      font-size: var(--text-body-sm);
    }
    .readonly-note { margin: var(--space-1) 0 0 0; font-size: var(--text-caption); color: var(--text-muted); }
    .form-actions { display: flex; gap: var(--space-2); margin-top: var(--space-5); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ProfileEditComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly profiles = inject(ProfileService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly countries = countryOptions();
  protected readonly timeZones = timeZoneOptions();
  protected readonly languages = languageOptions();
  protected readonly bioMax = BIO_MAX;

  protected readonly saving = signal(false);
  protected readonly uploading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fieldErrors = signal<Record<string, string>>({});

  protected readonly profile = this.profiles.profile;
  protected readonly displayName = this.profiles.displayName;

  protected readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.maxLength(100)]],
    lastName: ['', [Validators.maxLength(100)]],
    displayName: ['', [Validators.maxLength(100)]],
    phoneNumber: ['', [Validators.maxLength(32)]],
    country: [''],
    state: ['', [Validators.maxLength(100)]],
    city: ['', [Validators.maxLength(100)]],
    timeZone: [''],
    preferredLanguage: [''],
    bio: ['', [Validators.maxLength(BIO_MAX)]],
    linkedInUrl: ['', [Validators.maxLength(512)]],
    gitHubUrl: ['', [Validators.maxLength(512)]],
    portfolioUrl: ['', [Validators.maxLength(512)]],
  });

  /**
   * The form's current value as a signal.
   *
   * The design-system inputs take `[value]` rather than implementing
   * ControlValueAccessor, so the template has to read the control values directly.
   * Routing that through a signal keeps the bindings reactive to programmatic changes
   * — notably the `patchValue` that runs once the profile loads — instead of relying
   * on a change-detection pass happening to follow.
   */
  private readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  protected readonly bioLength = computed(() => (this.formValue().bio ?? '').length);

  constructor() {
    this.profiles.load().subscribe({
      next: (profile) =>
        this.form.patchValue({
          firstName: profile.firstName ?? '',
          lastName: profile.lastName ?? '',
          displayName: profile.displayName ?? '',
          phoneNumber: profile.phoneNumber ?? '',
          country: profile.country ?? '',
          state: profile.state ?? '',
          city: profile.city ?? '',
          // Prefill from the browser when unset, so a new user gets a sensible value
          // instead of an empty picker they have to hunt through.
          timeZone: profile.timeZone ?? detectedTimeZone(),
          preferredLanguage: profile.preferredLanguage ?? '',
          bio: profile.bio ?? '',
          linkedInUrl: profile.linkedInUrl ?? '',
          gitHubUrl: profile.gitHubUrl ?? '',
          portfolioUrl: profile.portfolioUrl ?? '',
        }),
      error: (error: unknown) => this.errorMessage.set(toProfileErrorMessage(error)),
    });
  }

  protected value(control: keyof ReturnType<typeof this.form.getRawValue>): string {
    return (this.formValue()[control] as string | undefined) ?? '';
  }

  protected set(control: string, value: string | number): void {
    this.form.get(control)?.setValue(String(value));
    this.form.get(control)?.markAsDirty();
  }

  protected errorFor(control: string): string | undefined {
    return this.fieldErrors()[control];
  }

  protected submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    const raw = this.form.getRawValue();

    // Empty strings become null so that clearing a field actually clears it. Sending
    // "" would store an empty string, which reads back as a set-but-blank value.
    const blank = (value: string) => (value.trim() === '' ? null : value.trim());

    this.profiles
      .update({
        firstName: blank(raw.firstName),
        lastName: blank(raw.lastName),
        displayName: blank(raw.displayName),
        phoneNumber: blank(raw.phoneNumber),
        country: blank(raw.country),
        state: blank(raw.state),
        city: blank(raw.city),
        timeZone: blank(raw.timeZone),
        preferredLanguage: blank(raw.preferredLanguage),
        bio: blank(raw.bio),
        linkedInUrl: blank(raw.linkedInUrl),
        gitHubUrl: blank(raw.gitHubUrl),
        portfolioUrl: blank(raw.portfolioUrl),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.form.markAsPristine();
          this.notifications.success('Profile saved');
          void this.router.navigate(['/profile/overview']);
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(toProfileErrorMessage(error));
          this.fieldErrors.set(toProfileFieldErrors(error));
        },
      });
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    this.uploading.set(true);
    this.errorMessage.set(null);

    this.profiles.uploadPicture(file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.notifications.success('Profile picture updated');
        // Reset so selecting the same file again still fires a change event.
        input.value = '';
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        input.value = '';
      },
    });
  }

  protected removePicture(): void {
    this.uploading.set(true);

    this.profiles.deletePicture().subscribe({
      next: () => {
        this.uploading.set(false);
        this.notifications.success('Profile picture removed');
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
      },
    });
  }
}
