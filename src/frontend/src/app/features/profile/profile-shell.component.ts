import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Event as RouterEvent } from '@angular/router';
import { filter, map } from 'rxjs';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { TabsComponent, TabItem } from '../../shared/components/tabs/tabs.component';

/**
 * Container for the profile and settings pages.
 *
 * Holds the heading and tab strip so they persist across tab changes rather than
 * being re-rendered by each child. The active tab is derived from the router rather
 * than tracked in local state, which keeps a deep link, a back button and a tab click
 * all producing the same result.
 */
@Component({
  selector: 'app-profile-shell',
  standalone: true,
  imports: [RouterOutlet, PageHeaderComponent, TabsComponent],
  template: `
    <app-page-header
      title="Profile & Settings"
      subtitle="Manage your personal details, security and preferences."
    />

    <app-tabs
      [items]="tabs"
      [activeId]="activeTab()"
      ariaLabel="Profile sections"
      (tabChange)="goTo($event)"
    />

    <div class="profile-content">
      <router-outlet />
    </div>
  `,
  styles: [`
    .profile-content {
      margin-top: var(--space-5);
      max-width: 880px;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ProfileShellComponent {
  private readonly router = inject(Router);

  protected readonly tabs: TabItem[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'edit', label: 'Edit Profile' },
    { id: 'account', label: 'Account' },
    { id: 'security', label: 'Security' },
    { id: 'notifications', label: 'Notifications' },
    { id: 'appearance', label: 'Appearance' },
  ];

  /**
   * Current tab, read from the URL.
   *
   * Seeded with the router's present URL rather than waiting for the first
   * NavigationEnd — otherwise a direct load of /profile/security would render with the
   * Overview tab highlighted until the next navigation.
   */
  private readonly url = toSignal(
    this.router.events.pipe(
      filter((event: RouterEvent): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );

  protected readonly activeTab = computed(() => {
    const segment = this.url().split('?')[0].split('/').filter(Boolean).pop();
    return this.tabs.some((tab) => tab.id === segment) ? segment! : 'overview';
  });

  protected goTo(tabId: string): void {
    void this.router.navigate(['/profile', tabId]);
  }
}
