import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';
import { appStateSignal } from '../../core/state/app.state';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [ButtonComponent, RouterLink],
  template: `
    <header class="app-header">
      <div class="header-left">
        <app-button
          variant="ghost"
          size="sm"
          (btnClick)="toggleSidebar()"
          aria-label="Toggle navigation drawer"
        >
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
          </svg>
        </app-button>
        <a routerLink="/" class="brand-logo">
          <span class="logo-badge">AI</span>
          <span class="logo-text">CareerPilot</span>
        </a>
      </div>

      <div class="header-right">
        <app-button
          variant="ghost"
          size="sm"
          (btnClick)="cycleTheme()"
          [aria-label]="'Current theme: ' + themeService.mode() + '. Click to change theme.'"
        >
          @if (themeService.resolved() === 'dark') {
            🌙 Dark
          } @else {
            ☀️ Light
          }
        </app-button>
      </div>
    </header>
  `,
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  protected readonly themeService = inject(ThemeService);

  protected toggleSidebar(): void {
    appStateSignal.update((state) => ({
      ...state,
      isSidebarOpen: !state.isSidebarOpen,
    }));
  }

  protected cycleTheme(): void {
    const current = this.themeService.mode();
    if (current === 'light') {
      this.themeService.setTheme('dark');
    } else if (current === 'dark') {
      this.themeService.setTheme('system');
    } else {
      this.themeService.setTheme('light');
    }
  }
}
