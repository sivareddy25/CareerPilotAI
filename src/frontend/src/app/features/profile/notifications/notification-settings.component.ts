import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ToggleComponent } from '../../../shared/components/toggle/toggle.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Preferences } from '../../../core/profile/profile.models';
import { toProfileErrorMessage } from '../profile-error';

interface ToggleRow {
  readonly key: keyof Pick<
    Preferences,
    'emailNotifications' | 'inAppNotifications' | 'weeklySummaryEmails' | 'marketingEmails'
  >;
  readonly label: string;
  readonly description: string;
}

/**
 * Notification preferences.
 *
 * Each toggle saves immediately and optimistically — there is no Save button. A
 * switch that does not move until a round-trip completes reads as broken, and these
 * settings have no server-side validation that could reject them, so the optimistic
 * state is always what the server will store. A failure reverts the switch and says so.
 */
@Component({
  selector: 'app-notification-settings',
  standalone: true,
  imports: [CardComponent, ToggleComponent],
  template: `
    @if (errorMessage(); as message) {
      <p class="form-alert" role="alert" aria-live="assertive">{{ message }}</p>
    }

    <app-card title="Notifications" subtitle="Changes save automatically.">
      @if (preferences(); as prefs) {
        <ul class="toggle-list">
          @for (row of rows; track row.key) {
            <li class="toggle-row">
              <div class="toggle-text">
                <span class="toggle-label">{{ row.label }}</span>
                <span class="toggle-description">{{ row.description }}</span>
              </div>
              <app-toggle
                [checked]="prefs[row.key]"
                [disabled]="saving()"
                [label]="row.label"
                (checkedChange)="toggle(row.key, $event)"
              />
            </li>
          }
        </ul>
      } @else {
        <p class="loading-note">Loading your preferences…</p>
      }
    </app-card>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; gap: var(--space-4); }
    .toggle-list { list-style: none; margin: 0; padding: 0; }
    .toggle-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      padding: var(--space-4) 0;
      border-bottom: 1px solid var(--border-color);
    }
    .toggle-row:last-child { border-bottom: none; }
    .toggle-text { display: flex; flex-direction: column; gap: 2px; }
    .toggle-label { font-size: var(--text-body-sm); font-weight: 500; color: var(--text-primary); }
    .toggle-description { font-size: var(--text-caption); color: var(--text-muted); max-width: 52ch; }
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
export default class NotificationSettingsComponent {
  private readonly profiles = inject(ProfileService);
  private readonly notifications = inject(NotificationService);

  protected readonly preferences = this.profiles.preferences;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly rows: ToggleRow[] = [
    {
      key: 'emailNotifications',
      label: 'Email notifications',
      description: 'Account and activity notices sent to your email address.',
    },
    {
      key: 'inAppNotifications',
      label: 'In-app notifications',
      description: 'Alerts shown while you are signed in.',
    },
    {
      key: 'weeklySummaryEmails',
      label: 'Weekly summary',
      description: 'A digest of your activity, once a week.',
    },
    {
      key: 'marketingEmails',
      label: 'Product news',
      description: 'Occasional announcements and product updates. Off by default.',
    },
  ];

  constructor() {
    this.profiles.load().subscribe({
      error: (error: unknown) => this.errorMessage.set(toProfileErrorMessage(error)),
    });
  }

  protected toggle(key: ToggleRow['key'], value: boolean): void {
    const current = this.preferences();
    if (!current) return;

    this.saving.set(true);
    this.errorMessage.set(null);

    // The service applies this locally before the request goes out and rolls it back
    // if the request fails, so the switch tracks the pointer immediately.
    this.profiles.updatePreferences({ ...current, [key]: value }).subscribe({
      next: () => this.saving.set(false),
      error: (error: unknown) => {
        this.saving.set(false);
        this.errorMessage.set(toProfileErrorMessage(error));
        this.notifications.error('Could not save that setting');
      },
    });
  }
}
