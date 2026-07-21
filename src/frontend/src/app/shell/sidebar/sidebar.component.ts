import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { appStateSignal } from '../../core/state/app.state';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <aside class="app-sidebar" [class.collapsed]="!appState().isSidebarOpen">
      <nav class="sidebar-nav" aria-label="Main Navigation">
        <ul class="nav-list">
          <li class="nav-item">
            <a
              routerLink="/"
              [routerLinkActiveOptions]="{ exact: true }"
              routerLinkActive="active"
              class="nav-link"
            >
              <span class="nav-icon">🏠</span>
              <span class="nav-text">Home</span>
            </a>
          </li>
        </ul>
      </nav>
    </aside>
  `,
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  protected readonly appState = appStateSignal;
}
