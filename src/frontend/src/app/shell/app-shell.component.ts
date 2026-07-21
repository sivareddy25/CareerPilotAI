import { Component, ChangeDetectionStrategy } from '@angular/core';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { FooterComponent } from './footer/footer.component';
import { PageContainerComponent } from './page-container/page-container.component';
import { RouterOutlet } from '@angular/router';
import { NotificationToastComponent } from '../shared/components/notification-toast/notification-toast.component';
import { LoadingOverlayComponent } from '../shared/components/loading-overlay/loading-overlay.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    HeaderComponent,
    SidebarComponent,
    FooterComponent,
    PageContainerComponent,
    RouterOutlet,
    NotificationToastComponent,
    LoadingOverlayComponent,
  ],
  template: `
    <a href="#main-content" class="skip-link">Skip to main content</a>
    <div class="shell-layout">
      <app-header />
      <div class="shell-body">
        <app-sidebar />
        <div class="shell-main flex-col">
          <app-page-container>
            <router-outlet />
          </app-page-container>
          <app-footer />
        </div>
      </div>
    </div>
    <app-notification-toast />
    <app-loading-overlay />
  `,
  styleUrl: './app-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {}
