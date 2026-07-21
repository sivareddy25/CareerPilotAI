import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { APP_CONSTANTS } from '../../core/constants/app.constants';

/**
 * Chrome for the unauthenticated pages.
 *
 * Deliberately carries no navigation, sidebar or user menu: the shell used elsewhere
 * renders links to areas a signed-out visitor cannot reach, and a menu of dead ends is
 * worse than none.
 */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="auth-layout">
      <main class="auth-panel">
        <header class="auth-brand">
          <h1 class="auth-brand-title">{{ appName }}</h1>
          <p class="auth-brand-tagline">Intelligent career platform</p>
        </header>

        <router-outlet />
      </main>
    </div>
  `,
  styleUrl: './auth-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthLayoutComponent {
  protected readonly appName = APP_CONSTANTS.TITLE;
}
