import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { AppConfigService } from '../../core/config/app-config.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="app-footer">
      <div class="footer-content">
        <p>&copy; {{ currentYear }} {{ configService.appName }}. All rights reserved.</p>
        <span class="version-badge">v{{ configService.version }}</span>
      </div>
    </footer>
  `,
  styleUrl: './footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FooterComponent {
  protected readonly configService = inject(AppConfigService);
  protected readonly currentYear = new Date().getFullYear();
}
