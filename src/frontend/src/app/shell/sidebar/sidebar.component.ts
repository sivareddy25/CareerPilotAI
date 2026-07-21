import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { appStateSignal } from '../../core/state/app.state';
import { HostingService } from '../../core/services/hosting.service';
import { IconComponent, IconName } from '../../shared/components/icon/icon.component';
import { BadgeComponent } from '../../shared/components/badge/badge.component';

export interface SidebarNavItem {
  label: string;
  url: string;
  icon: IconName;
  badge?: string;
  exact?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, IconComponent, BadgeComponent],
  template: `
    <aside class="app-sidebar" [class.collapsed]="!appState().isSidebarOpen">
      <div class="sidebar-content">
        <nav class="sidebar-nav" aria-label="Main Navigation">
          <div class="nav-section-label">Navigation</div>
          <ul class="nav-list">
            @for (item of mainNavItems; track item.url) {
              <li class="nav-item">
                <a
                  [routerLink]="item.url"
                  [routerLinkActiveOptions]="{ exact: !!item.exact }"
                  routerLinkActive="active"
                  class="nav-link"
                >
                  <app-icon [name]="item.icon" size="md" class="nav-icon" />
                  <span class="nav-text">{{ item.label }}</span>
                  @if (item.badge) {
                    <app-badge variant="primary" styleMode="soft" class="nav-badge">{{ item.badge }}</app-badge>
                  }
                </a>
              </li>
            }
          </ul>
        </nav>

        <div class="sidebar-footer">
          <div class="system-status">
            <span class="status-dot"></span>
            <span class="status-text">{{ hosting.isLocalMode() ? 'Single User (Local Mode)' : 'SaaS Enterprise v1.0' }}</span>
          </div>
        </div>
      </div>
    </aside>
  `,
  styles: [`
    .app-sidebar {
      width: 260px;
      height: calc(100vh - 64px);
      background-color: var(--bg-secondary);
      border-right: 1px solid var(--border-color);
      transition: width var(--duration-normal) var(--ease-fluent);
      overflow-x: hidden;

      &.collapsed {
        width: 0;
        border-right-color: transparent;
      }
    }
    .sidebar-content {
      width: 260px;
      height: 100%;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: var(--space-4);
      box-sizing: border-box;
    }
    .nav-section-label {
      font-size: var(--text-caption);
      font-weight: 600;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      padding: 0 var(--space-3) var(--space-2) var(--space-3);
    }
    .nav-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    .nav-link {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-md);
      color: var(--text-secondary);
      text-decoration: none;
      font-size: var(--text-body-sm);
      font-weight: 500;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover {
        background-color: var(--bg-tertiary);
        color: var(--text-primary);
      }

      &.active {
        background-color: var(--brand-primary-alpha);
        color: var(--brand-primary);
        font-weight: 600;
      }
    }
    .nav-icon { color: inherit; }
    .nav-text { flex: 1; }
    .sidebar-footer {
      padding: var(--space-3);
      border-top: 1px solid var(--border-subtle);
    }
    .system-status {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-circle);
      background-color: var(--success);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  protected readonly appState = appStateSignal;
  protected readonly hosting = inject(HostingService);

  protected readonly mainNavItems: SidebarNavItem[] = [
    { label: 'Overview', url: '/', icon: 'grid', exact: true },
    { label: 'Recruiter Hub', url: '/communication', icon: 'mail', badge: 'AI' },
    { label: 'Job Aggregator', url: '/jobs', icon: 'search', badge: 'New', exact: true },
    { label: 'Hiring Companies', url: '/jobs/companies', icon: 'building' },
    { label: 'Saved Jobs', url: '/jobs/saved', icon: 'file-text' },
    { label: 'Template Gallery', url: '/resumes/templates', icon: 'file-text' },
    { label: 'Live Preview', url: '/resumes/preview', icon: 'eye' },
    { label: 'Import Wizard', url: '/resumes/import', icon: 'upload' },
    { label: 'System Health', url: '/system/settings', icon: 'settings' },
    { label: 'Design System', url: '/design-system', icon: 'sparkles', badge: 'Fluent 2' },
  ];
}
