import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="app-footer">
      <div class="footer-container">
        <p class="copyright">&copy; {{ currentYear }} CareerPilot AI. Production Design System infrastructure.</p>
        <div class="footer-links">
          <a href="#" class="footer-link">Accessibility</a>
          <a href="#" class="footer-link">Privacy</a>
          <a href="#" class="footer-link">Terms</a>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .app-footer {
      padding: var(--space-4) var(--space-6);
      background-color: var(--bg-primary);
      border-top: 1px solid var(--border-subtle);
      margin-top: auto;
    }
    .footer-container {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      flex-wrap: wrap;
    }
    .copyright {
      margin: 0;
      font-size: var(--text-caption);
      color: var(--text-muted);
    }
    .footer-links {
      display: flex;
      gap: var(--space-4);
    }
    .footer-link {
      font-size: var(--text-caption);
      color: var(--text-secondary);
      text-decoration: none;

      &:hover {
        color: var(--brand-primary);
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FooterComponent {
  protected readonly currentYear = new Date().getFullYear();
}
