import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { appStateSignal } from '../../core/state/app.state';
import { HostingService } from '../../core/services/hosting.service';
import { IconComponent, IconName } from '../../shared/components/icon/icon.component';
import { BadgeComponent } from '../../shared/components/badge/badge.component';
import { LogoComponent } from '../../shared/components/logo/logo.component';

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
  imports: [CommonModule, RouterLink, RouterLinkActive, IconComponent, BadgeComponent, LogoComponent],
  template: `
    <aside class="app-sidebar" [class.collapsed]="!appState().isSidebarOpen">
      <div class="sidebar-header">
        <app-logo [showText]="appState().isSidebarOpen" size="md" />
      </div>

      <div class="sidebar-content">
        <nav class="sidebar-nav" aria-label="Main Navigation">
          <div class="nav-section-label" *ngIf="appState().isSidebarOpen">Platform</div>
          <ul class="nav-list">
            @for (item of mainNavItems; track item.url) {
              <li class="nav-item">
                <a
                  [routerLink]="item.url"
                  [routerLinkActiveOptions]="{ exact: !!item.exact }"
                  routerLinkActive="active"
                  class="nav-link"
                  [title]="!appState().isSidebarOpen ? item.label : ''"
                >
                  <app-icon [name]="item.icon" size="md" class="nav-icon" />
                  <span class="nav-text" *ngIf="appState().isSidebarOpen">{{ item.label }}</span>
                  @if (item.badge && appState().isSidebarOpen) {
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
            <span class="status-text" *ngIf="appState().isSidebarOpen">
              {{ hosting.isLocalMode() ? 'Single User (Local Mode)' : 'SaaS Enterprise v1.0' }}
            </span>
          </div>
        </div>
      </div>
    </aside>
  `,
  styles: [`
    .app-sidebar {
      width: 260px;
      height: 100vh;
      background-color: var(--bg-secondary);
      border-right: 1px solid var(--border-color);
      transition: width var(--duration-normal) var(--ease-fluent);
      overflow-x: hidden;
      display: flex;
      flex-direction: column;

      &.collapsed {
        width: 72px;
        .sidebar-header { padding: var(--space-4) var(--space-3); justify-content: center; }
        .nav-link { justify-content: center; padding: var(--space-3); }
        .sidebar-footer { justify-content: center; }
      }
    }
    .sidebar-header {
      padding: var(--space-4) var(--space-6);
      border-bottom: 1px solid var(--border-subtle);
      display: flex;
      align-items: center;
    }
    .sidebar-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: var(--space-4);
      box-sizing: border-box;
      overflow-y: auto;
    }
    .nav-section-label {
      font-size: var(--text-caption);
      font-weight: 700;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.8px;
      padding: var(--space-2) var(--space-3);
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
    .nav-text { flex: 1; white-space: nowrap; }
    .sidebar-footer {
      padding: var(--space-3);
      border-top: 1px solid var(--border-subtle);
      display: flex;
      align-items: center;
    }
    .system-status {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-caption);
      color: var(--text-muted);
      white-space: nowrap;
    }
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-circle);
      background-color: var(--success);
      flex-shrink: 0;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  protected readonly appState = appStateSignal;
  protected readonly hosting = inject(HostingService);

  protected readonly mainNavItems: SidebarNavItem[] = [
    { label: 'Dashboard', url: '/', icon: 'grid', exact: true },
    { label: 'Search Jobs', url: '/jobs', icon: 'briefcase', badge: 'New', exact: true },
    { label: 'Saved Jobs', url: '/jobs/saved', icon: 'bookmark' },
    { label: 'Applications', url: '/jobs/saved', icon: 'check-circle' },
    { label: 'Resume Manager', url: '/resumes/templates', icon: 'file-text' },
    { label: 'Cover Letters', url: '/resumes/templates', icon: 'layers' },
    { label: 'AI Recruiter Hub', url: '/communication', icon: 'mail', badge: 'AI' },
    { label: 'Analytics', url: '/', icon: 'activity' },
    { label: 'System Health', url: '/system/settings', icon: 'cpu' },
    { label: 'Settings', url: '/profile/account', icon: 'settings' },
  ];
}
