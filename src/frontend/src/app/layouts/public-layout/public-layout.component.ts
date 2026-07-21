import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router';
import { FooterComponent } from '../../shell/footer/footer.component';
import { TopNavComponent, TopNavItem } from '../../shared/components/top-nav/top-nav.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { ThemeService } from '../../core/services/theme.service';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';

@Component({
  selector: 'app-landing-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    FooterComponent,
    TopNavComponent,
    ButtonComponent,
    IconComponent,
    IconButtonComponent,
  ],
  template: `
    <div class="landing-layout">
      <header class="landing-header">
        <div class="header-container">
          <a routerLink="/" class="brand">
            <div class="brand-logo">
              <app-icon name="sparkles" size="sm" />
            </div>
            <span class="brand-name">CareerPilot<span class="accent">AI</span></span>
          </a>

          <app-top-nav [items]="navItems" />

          <div class="header-actions">
            <app-icon-button
              [icon]="themeService.resolved() === 'dark' ? 'moon' : 'sun'"
              size="md"
              ariaLabel="Toggle theme"
              (btnClick)="toggleTheme()"
            />
            <a routerLink="/design-system">
              <app-button variant="outline" size="sm">Design System</app-button>
            </a>
            <a routerLink="/auth/login">
              <app-button variant="primary" size="sm">Sign In</app-button>
            </a>
          </div>
        </div>
      </header>

      <main class="landing-main">
        <router-outlet />
      </main>

      <app-footer />
    </div>
  `,
  styles: [`
    .landing-layout {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: var(--bg-primary);
    }
    .landing-header {
      height: 72px;
      display: flex;
      align-items: center;
      padding: 0 var(--space-6);
      background-color: var(--bg-glass);
      backdrop-filter: blur(16px);
      border-bottom: 1px solid var(--border-color);
      position: sticky;
      top: 0;
      z-index: var(--z-header);
    }
    .header-container {
      width: 100%;
      max-width: 1280px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .brand {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      text-decoration: none;
      color: var(--text-primary);
      font-weight: 700;
      font-size: var(--text-h4);
    }
    .brand-logo {
      width: 34px;
      height: 34px;
      border-radius: var(--radius-md);
      background: linear-gradient(135deg, var(--brand-primary), #818cf8);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .accent { color: var(--brand-primary); }
    .header-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }
    .landing-main {
      flex: 1;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingLayoutComponent {
  protected readonly themeService = inject(ThemeService);

  protected readonly navItems: TopNavItem[] = [
    { label: 'Features', url: '#' },
    { label: 'Design System', url: '/design-system' },
    { label: 'Documentation', url: '#' },
  ];

  protected toggleTheme(): void {
    const current = this.themeService.resolved();
    this.themeService.setTheme(current === 'dark' ? 'light' : 'dark');
  }
}

export { LandingLayoutComponent as PublicLayoutComponent };
