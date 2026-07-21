import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { IconComponent } from '../../shared/components/icon/icon.component';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, IconButtonComponent, IconComponent],
  template: `
    <div class="auth-layout-grid">
      <!-- Left Hero Banner -->
      <div class="auth-banner">
        <div class="banner-content">
          <div class="brand-logo-large">
            <app-icon name="sparkles" size="lg" />
          </div>
          <h1 class="banner-title">CareerPilot AI</h1>
          <p class="banner-subtitle">
            Production-grade design infrastructure built with Angular 22 & Microsoft Fluent 2 aesthetics.
          </p>

          <div class="feature-badges">
            <div class="feature-tag"><app-icon name="check" size="xs" /> WCAG 2.2 AA Compliant</div>
            <div class="feature-tag"><app-icon name="check" size="xs" /> High Contrast Mode</div>
            <div class="feature-tag"><app-icon name="check" size="xs" /> CSS Variables System</div>
          </div>
        </div>
      </div>

      <!-- Right Form Panel -->
      <div class="auth-form-panel">
        <div class="auth-top-bar">
          <app-icon-button
            [icon]="themeService.resolved() === 'dark' ? 'moon' : 'sun'"
            size="md"
            ariaLabel="Toggle theme"
            (btnClick)="toggleTheme()"
          />
        </div>

        <div class="form-container">
          <router-outlet />
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-layout-grid {
      display: grid;
      grid-template-columns: 1.2fr 1fr;
      min-height: 100vh;
      background-color: var(--bg-primary);

      @media (max-width: 960px) {
        grid-template-columns: 1fr;
      }
    }
    .auth-banner {
      background: linear-gradient(135deg, var(--brand-primary), #4338ca);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-8);

      @media (max-width: 960px) {
        display: none;
      }
    }
    .banner-content {
      max-width: 480px;
    }
    .brand-logo-large {
      width: 56px;
      height: 56px;
      border-radius: var(--radius-xl);
      background-color: rgba(255, 255, 255, 0.2);
      backdrop-filter: blur(10px);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: var(--space-4);
    }
    .banner-title {
      font-size: var(--text-display);
      font-weight: 700;
      margin: 0 0 var(--space-2) 0;
    }
    .banner-subtitle {
      font-size: var(--text-body-lg);
      opacity: 0.9;
      line-height: var(--lh-body-lg);
      margin-bottom: var(--space-6);
    }
    .feature-badges {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    .feature-tag {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-body-sm);
      opacity: 0.95;
    }
    .auth-form-panel {
      display: flex;
      flex-direction: column;
      padding: var(--space-6);
      position: relative;
    }
    .auth-top-bar {
      display: flex;
      justify-content: flex-end;
    }
    .form-container {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthLayoutComponent {
  protected readonly themeService = inject(ThemeService);

  protected toggleTheme(): void {
    const current = this.themeService.resolved();
    this.themeService.setTheme(current === 'dark' ? 'light' : 'dark');
  }
}
