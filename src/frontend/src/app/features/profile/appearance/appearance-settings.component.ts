import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CardComponent } from '../../../shared/components/card/card.component';
import { SelectComponent, SelectOption } from '../../../shared/components/select/select.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  DateFormatPreference,
  Preferences,
  ThemePreference,
  TimeFormatPreference,
} from '../../../core/profile/profile.models';
import { toProfileErrorMessage } from '../profile-error';

/**
 * Theme and formatting preferences.
 *
 * Like notifications, each change saves immediately and optimistically. Theme in
 * particular has to: the whole point is seeing the new theme, and applying it only
 * after a round-trip would make the control feel unresponsive.
 */
@Component({
  selector: 'app-appearance-settings',
  standalone: true,
  imports: [CardComponent, SelectComponent],
  template: `
    @if (errorMessage(); as message) {
      <p class="form-alert" role="alert" aria-live="assertive">{{ message }}</p>
    }

    <app-card title="Appearance" subtitle="Changes save automatically and apply everywhere you sign in.">
      @if (preferences(); as prefs) {
        <div class="settings-grid">
          <app-select
            label="Theme"
            [options]="themeOptions"
            [value]="prefs.theme"
            [disabled]="saving()"
            helperText="System follows your device setting."
            (valueChange)="update('theme', $event)"
          />
          <app-select
            label="Date format"
            [options]="dateFormatOptions"
            [value]="prefs.dateFormat"
            [disabled]="saving()"
            (valueChange)="update('dateFormat', $event)"
          />
          <app-select
            label="Time format"
            [options]="timeFormatOptions"
            [value]="prefs.timeFormat"
            [disabled]="saving()"
            (valueChange)="update('timeFormat', $event)"
          />
        </div>

        <p class="preview">
          Preview: <strong>{{ preview() }}</strong>
        </p>
      } @else {
        <p class="loading-note">Loading your preferences…</p>
      }
    </app-card>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; gap: var(--space-4); }
    .settings-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: var(--space-4);
    }
    .preview {
      margin: var(--space-5) 0 0 0;
      padding: var(--space-3);
      background-color: var(--bg-tertiary);
      border-radius: var(--radius-md);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
    }
    .loading-note { color: var(--text-muted); font-size: var(--text-body-sm); margin: 0; }
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
export default class AppearanceSettingsComponent {
  private readonly profiles = inject(ProfileService);
  private readonly notifications = inject(NotificationService);

  protected readonly preferences = this.profiles.preferences;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly themeOptions: SelectOption[] = [
    { value: ThemePreference.System, label: 'System' },
    { value: ThemePreference.Light, label: 'Light' },
    { value: ThemePreference.Dark, label: 'Dark' },
    { value: ThemePreference.HighContrast, label: 'High contrast' },
  ];

  protected readonly dateFormatOptions: SelectOption[] = [
    { value: DateFormatPreference.IsoYearMonthDay, label: '2026-07-21' },
    { value: DateFormatPreference.DayMonthYear, label: '21/07/2026' },
    { value: DateFormatPreference.MonthDayYear, label: '07/21/2026' },
    { value: DateFormatPreference.DayMonthNameYear, label: '21 Jul 2026' },
  ];

  protected readonly timeFormatOptions: SelectOption[] = [
    { value: TimeFormatPreference.TwentyFourHour, label: '24-hour (14:30)' },
    { value: TimeFormatPreference.TwelveHour, label: '12-hour (2:30 PM)' },
  ];

  constructor() {
    this.profiles.load().subscribe({
      error: (error: unknown) => this.errorMessage.set(toProfileErrorMessage(error)),
    });
  }

  /** Renders a sample using the chosen formats, so the effect is visible before saving matters. */
  protected preview(): string {
    const prefs = this.preferences();
    if (!prefs) return '';

    const sample = new Date(2026, 6, 21, 14, 30);
    const pad = (n: number) => String(n).padStart(2, '0');

    const date = {
      [DateFormatPreference.IsoYearMonthDay]:
        `${sample.getFullYear()}-${pad(sample.getMonth() + 1)}-${pad(sample.getDate())}`,
      [DateFormatPreference.DayMonthYear]:
        `${pad(sample.getDate())}/${pad(sample.getMonth() + 1)}/${sample.getFullYear()}`,
      [DateFormatPreference.MonthDayYear]:
        `${pad(sample.getMonth() + 1)}/${pad(sample.getDate())}/${sample.getFullYear()}`,
      [DateFormatPreference.DayMonthNameYear]:
        `${pad(sample.getDate())} ${sample.toLocaleString(undefined, { month: 'short' })} ${sample.getFullYear()}`,
    }[prefs.dateFormat];

    const time =
      prefs.timeFormat === TimeFormatPreference.TwelveHour
        ? sample.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit', hour12: true })
        : `${pad(sample.getHours())}:${pad(sample.getMinutes())}`;

    return `${date} ${time}`;
  }

  protected update(key: keyof Preferences, value: string | number): void {
    const current = this.preferences();
    if (!current) return;

    this.saving.set(true);
    this.errorMessage.set(null);

    // Select emits its value as a string; the server's enums are numeric, so this has
    // to be coerced or the payload fails model binding.
    this.profiles.updatePreferences({ ...current, [key]: Number(value) }).subscribe({
      next: () => this.saving.set(false),
      error: (error: unknown) => {
        this.saving.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        this.notifications.error('Could not save that setting');
      },
    });
  }
}
