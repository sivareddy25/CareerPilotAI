import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { ProfileService } from '../../../core/profile/profile.service';
import { toProfileErrorMessage } from '../profile-error';

/**
 * Read-only summary of the user's profile.
 *
 * Deliberately has no form and no save. Separating viewing from editing means the
 * common case — checking what is on file — cannot accidentally submit a change, and
 * the page can render before any form machinery is built.
 */
@Component({
  selector: 'app-profile-overview',
  standalone: true,
  imports: [RouterLink, AvatarComponent, ButtonComponent, CardComponent, IconComponent, SkeletonComponent],
  template: `
    @if (loading()) {
      <app-card>
        <div class="skeleton-stack">
          <app-skeleton width="120px" height="120px" />
          <app-skeleton width="60%" height="24px" />
          <app-skeleton width="40%" height="16px" />
        </div>
      </app-card>
    } @else if (error(); as message) {
      <app-card>
        <p class="load-error" role="alert">{{ message }}</p>
        <app-button variant="secondary" (btnClick)="reload()">Try again</app-button>
      </app-card>
    } @else if (profile(); as data) {
      <app-card>
        <div class="identity">
          <app-avatar
            [src]="data.profilePictureUrl ?? undefined"
            [name]="displayName()"
            size="xl"
          />
          <div class="identity-text">
            <h2 class="identity-name">{{ displayName() }}</h2>
            <p class="identity-email">
              {{ data.email }}
              @if (data.emailConfirmed) {
                <span class="verified" title="Email verified">
                  <app-icon name="check-circle" size="xs" /> Verified
                </span>
              }
            </p>
            @if (data.bio) {
              <p class="identity-bio">{{ data.bio }}</p>
            }
          </div>
          <app-button variant="primary" routerLink="/profile/edit">Edit profile</app-button>
        </div>
      </app-card>

      <app-card title="Details">
        <dl class="detail-grid">
          @for (item of details(); track item.label) {
            <div class="detail-row">
              <dt>{{ item.label }}</dt>
              <dd [class.empty]="!item.value">{{ item.value || 'Not set' }}</dd>
            </div>
          }
        </dl>
      </app-card>

      @if (links().length > 0) {
        <app-card title="Links">
          <ul class="link-list">
            @for (link of links(); track link.url) {
              <li>
                <app-icon [name]="link.icon" size="sm" />
                <!--
                  noopener is the security-relevant half: without it the opened page
                  can reach back through window.opener and navigate this tab. These are
                  user-supplied URLs, so that matters. The server has already restricted
                  the scheme to http(s).
                -->
                <a [href]="link.url" target="_blank" rel="noopener noreferrer">{{ link.label }}</a>
              </li>
            }
          </ul>
        </app-card>
      }
    }
  `,
  styles: [`
    :host { display: flex; flex-direction: column; gap: var(--space-4); }
    .skeleton-stack { display: flex; flex-direction: column; gap: var(--space-3); }
    .load-error { color: var(--danger); margin: 0 0 var(--space-3) 0; }

    .identity {
      display: flex;
      align-items: flex-start;
      gap: var(--space-4);
      flex-wrap: wrap;
    }
    .identity-text { flex: 1; min-width: 200px; }
    .identity-name {
      margin: 0;
      font-size: var(--text-title);
      font-weight: 600;
      color: var(--text-primary);
    }
    .identity-email {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      margin: var(--space-1) 0 0 0;
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      flex-wrap: wrap;
    }
    .verified {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      color: var(--success, #16a34a);
      font-size: var(--text-caption);
    }
    .identity-bio {
      margin: var(--space-3) 0 0 0;
      color: var(--text-secondary);
      line-height: var(--lh-body);
      /* User-authored free text: wrap hard so a long unbroken string cannot widen
         the layout past the viewport. */
      overflow-wrap: anywhere;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: var(--space-4);
      margin: 0;
    }
    .detail-row dt {
      font-size: var(--text-caption);
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: var(--text-muted);
      margin-bottom: 2px;
    }
    .detail-row dd {
      margin: 0;
      color: var(--text-primary);
      font-size: var(--text-body-sm);
      overflow-wrap: anywhere;
    }
    .detail-row dd.empty { color: var(--text-muted); font-style: italic; }

    .link-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--space-2); }
    .link-list li { display: flex; align-items: center; gap: var(--space-2); }
    .link-list a { color: var(--brand-primary); overflow-wrap: anywhere; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ProfileOverviewComponent {
  private readonly profiles = inject(ProfileService);

  protected readonly profile = this.profiles.profile;
  protected readonly displayName = this.profiles.displayName;
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly details = computed(() => {
    const data = this.profile();
    if (!data) return [];

    return [
      { label: 'First name', value: data.firstName ?? '' },
      { label: 'Last name', value: data.lastName ?? '' },
      { label: 'Display name', value: data.displayName ?? '' },
      { label: 'Phone', value: data.phoneNumber ?? '' },
      { label: 'City', value: data.city ?? '' },
      { label: 'State', value: data.state ?? '' },
      { label: 'Country', value: data.country ?? '' },
      { label: 'Time zone', value: data.timeZone ?? '' },
      { label: 'Language', value: data.preferredLanguage ?? '' },
    ];
  });

  protected readonly links = computed(() => {
    const data = this.profile();
    if (!data) return [];

    return [
      { label: 'LinkedIn', url: data.linkedInUrl, icon: 'external-link' as const },
      { label: 'GitHub', url: data.gitHubUrl, icon: 'external-link' as const },
      { label: 'Portfolio', url: data.portfolioUrl, icon: 'external-link' as const },
    ].filter((link): link is { label: string; url: string; icon: 'external-link' } => !!link.url);
  });

  constructor() {
    this.reload();
  }

  protected reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.profiles.load().subscribe({
      next: () => this.loading.set(false),
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(toProfileErrorMessage(error));
      },
    });
  }
}
