import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type LogoSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="logo-container" [class]="'logo-' + size()">
      <svg
        class="logo-icon"
        viewBox="0 0 32 32"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
      >
        <!-- Background Shield Badge -->
        <rect width="32" height="32" rx="8" fill="url(#logo_grad)" />
        
        <!-- Briefcase Outline -->
        <rect x="7" y="12" width="18" height="13" rx="2" stroke="#ffffff" stroke-width="2" />
        <path d="M11 12V9A2 2 0 0 1 13 7H19A2 2 0 0 1 21 9V12" stroke="#ffffff" stroke-width="2" />
        
        <!-- AI Compass Spark -->
        <path d="M16 14L17.5 17.5L21 19L17.5 20.5L16 24L14.5 20.5L11 19L14.5 17.5L16 14Z" fill="#8B5CF6" />
        <circle cx="16" cy="19" r="1.5" fill="#ffffff" />
        
        <defs>
          <linearGradient id="logo_grad" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
            <stop stop-color="#0066FF" />
            <stop offset="1" stop-color="#4F46E5" />
          </linearGradient>
        </defs>
      </svg>
      
      @if (showText()) {
        <div class="logo-brand">
          <span class="brand-title">CareerPilot</span>
          <span class="brand-ai">AI</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .logo-container {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      user-select: none;
    }
    .logo-icon {
      flex-shrink: 0;
      transition: transform 0.2s ease;
    }
    .logo-container:hover .logo-icon {
      transform: scale(1.05);
    }
    
    .logo-sm .logo-icon { width: 24px; height: 24px; }
    .logo-md .logo-icon { width: 32px; height: 32px; }
    .logo-lg .logo-icon { width: 44px; height: 44px; }
    
    .logo-brand {
      display: flex;
      align-items: center;
      gap: 4px;
      font-weight: 700;
      letter-spacing: -0.5px;
      line-height: 1;
    }
    .logo-sm .logo-brand { font-size: 16px; }
    .logo-md .logo-brand { font-size: 20px; }
    .logo-lg .logo-brand { font-size: 26px; }

    .brand-title {
      color: var(--text-primary);
    }
    .brand-ai {
      background: linear-gradient(135deg, #0066FF 0%, #8B5CF6 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      font-weight: 800;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoComponent {
  readonly size = input<LogoSize>('md');
  readonly showText = input<boolean>(true);
}
