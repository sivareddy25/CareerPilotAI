import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { appStateSignal } from '../../core/state/app.state';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { CommandBarComponent } from '../../shared/components/command-bar/command-bar.component';
import { DropdownComponent, DropdownItem } from '../../shared/components/dropdown/dropdown.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IconComponent,
    IconButtonComponent,
    AvatarComponent,
    CommandBarComponent,
    DropdownComponent,
  ],
  template: `
    <header class="app-header">
      <div class="header-left">
        <app-icon-button
          icon="menu"
          size="md"
          ariaLabel="Toggle Navigation Sidebar"
          (btnClick)="toggleSidebar()"
        />
        <a routerLink="/" class="brand-logo">
          <div class="logo-mark">
            <app-icon name="sparkles" size="sm" />
          </div>
          <span class="logo-text">CareerPilot<span class="logo-accent">AI</span></span>
        </a>
      </div>

      <div class="header-center">
        <button type="button" class="command-trigger" (click)="isCommandOpen.set(true)">
          <app-icon name="search" size="sm" class="search-icon" />
          <span class="placeholder-text">Search commands, pages...</span>
          <kbd class="cmd-kbd">⌘K</kbd>
        </button>
      </div>

      <div class="header-right">
        <app-dropdown [items]="themeOptions" (itemSelect)="onThemeSelect($event)">
          <app-icon-button
            trigger
            [icon]="getThemeIcon()"
            size="md"
            [ariaLabel]="'Theme: ' + themeService.mode()"
          />
        </app-dropdown>

        <a routerLink="/design-system" class="ds-link" title="Design System Showcase">
          <app-icon name="grid" size="md" />
        </a>

        <div class="user-profile">
          <app-avatar name="Principal Designer" size="sm" status="online" />
        </div>
      </div>
    </header>

    <app-command-bar [isOpen]="isCommandOpen()" (closed)="isCommandOpen.set(false)" />
  `,
  styles: [`
    .app-header {
      height: 64px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 var(--space-6);
      background-color: var(--bg-glass);
      backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--border-color);
      position: sticky;
      top: 0;
      z-index: var(--z-header);
    }
    .header-left {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    .brand-logo {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      text-decoration: none;
      color: var(--text-primary);
      font-weight: 700;
      font-size: var(--text-h4);
    }
    .logo-mark {
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      background: linear-gradient(135deg, var(--brand-primary), #818cf8);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .logo-accent {
      color: var(--brand-primary);
      margin-left: 2px;
    }
    .header-center {
      flex: 1;
      max-width: 480px;
      margin: 0 var(--space-6);

      @media (max-width: 768px) { display: none; }
    }
    .command-trigger {
      width: 100%;
      height: 38px;
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: 0 var(--space-3);
      background-color: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      color: var(--text-muted);
      cursor: pointer;
      font-size: var(--text-body-sm);
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover {
        border-color: var(--border-strong);
        background-color: var(--bg-elevated);
      }
    }
    .search-icon { color: var(--text-muted); }
    .placeholder-text { flex: 1; text-align: left; }
    .cmd-kbd {
      padding: 2px 6px;
      font-size: var(--text-caption);
      font-family: var(--font-mono);
      color: var(--text-muted);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xs);
    }
    .header-right {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }
    .ds-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      color: var(--text-secondary);
      border-radius: var(--radius-md);
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover {
        background-color: var(--bg-tertiary);
        color: var(--brand-primary);
      }
    }
    .user-profile {
      cursor: pointer;
      padding: 2px;
      border-radius: var(--radius-circle);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  protected readonly themeService = inject(ThemeService);
  protected isCommandOpen = signal<boolean>(false);

  protected readonly themeOptions: DropdownItem[] = [
    { id: 'light', label: 'Light Theme', icon: 'sun' },
    { id: 'dark', label: 'Dark Theme', icon: 'moon' },
    { id: 'system', label: 'System Theme', icon: 'monitor' },
    { id: 'high-contrast', label: 'High Contrast', icon: 'eye' },
  ];

  protected toggleSidebar(): void {
    appStateSignal.update((state) => ({
      ...state,
      isSidebarOpen: !state.isSidebarOpen,
    }));
  }

  protected onThemeSelect(id: string): void {
    this.themeService.setTheme(id as any);
  }

  protected getThemeIcon(): any {
    const mode = this.themeService.mode();
    if (mode === 'dark') return 'moon';
    if (mode === 'light') return 'sun';
    if (mode === 'high-contrast') return 'eye';
    return 'monitor';
  }
}
