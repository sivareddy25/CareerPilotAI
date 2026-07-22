import { Component, ChangeDetectionStrategy, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../shared/components';

export interface ProfileOption {
  id: string;
  name: string;
  title: string;
  skills: string;
  isPrimary?: boolean;
}

@Component({
  selector: 'app-multi-profile-selector',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="profile-selector-bar">
      <div class="selector-label">
        <app-icon name="user" size="xs" />
        <span>Active Career Profile:</span>
      </div>

      <div class="profiles-pills-row">
        @for (p of profiles(); track p.id) {
          <button
            type="button"
            class="profile-pill"
            [class.active]="activeProfileId() === p.id"
            (click)="selectProfile(p)"
          >
            <span class="profile-name">{{ p.name }}</span>
            <span class="profile-title">({{ p.title }})</span>
            @if (p.isPrimary) {
              <span class="primary-tag">Primary</span>
            }
          </button>
        }

        <button type="button" class="add-profile-btn" (click)="addProfileClick.emit()">
          <app-icon name="plus" size="xs" /> New Profile
        </button>
      </div>
    </div>
  `,
  styles: [`
    .profile-selector-bar {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      margin-bottom: var(--space-4);
      padding: var(--space-2) var(--space-4);
      background-color: var(--surface-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
    }
    .selector-label {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-caption);
      font-weight: 600;
      color: var(--text-secondary);
      white-space: nowrap;
    }
    .profiles-pills-row {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      flex-wrap: wrap;
    }
    .profile-pill {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-full);
      border: 1px solid var(--border-color);
      background-color: var(--surface-muted);
      color: var(--text-secondary);
      font-size: var(--text-caption);
      cursor: pointer;
      transition: all 0.2s ease;
      &:hover {
        border-color: var(--brand-primary);
        color: var(--brand-primary);
      }
      &.active {
        background-color: var(--brand-primary);
        color: #ffffff;
        border-color: var(--brand-primary);
        font-weight: 600;
        .profile-title { color: rgba(255, 255, 255, 0.85); }
        .primary-tag { background: rgba(255, 255, 255, 0.25); color: #fff; }
      }
    }
    .profile-name { font-weight: 600; }
    .profile-title { font-size: 11px; opacity: 0.8; }
    .primary-tag {
      font-size: 10px;
      padding: 1px 6px;
      border-radius: 4px;
      background: var(--brand-primary-alpha);
      color: var(--brand-primary);
    }
    .add-profile-btn {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-full);
      border: 1px dashed var(--border-color);
      background: transparent;
      color: var(--brand-primary);
      font-size: var(--text-caption);
      font-weight: 600;
      cursor: pointer;
      &:hover {
        background-color: var(--brand-primary-alpha);
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MultiProfileSelectorComponent {
  readonly profiles = signal<ProfileOption[]>([
    {
      id: 'profile-1',
      name: 'Venkata (Full Stack)',
      title: '.NET & Angular',
      skills: '.NET, C#, ASP.NET Core, Angular, TypeScript, SQL',
      isPrimary: true,
    },
    {
      id: 'profile-2',
      name: 'Venkata (Frontend Lead)',
      title: 'Angular 18 & Web UI',
      skills: 'Angular, TypeScript, HTML/CSS, RxJS, REST API',
      isPrimary: false,
    },
    {
      id: 'profile-3',
      name: 'Venkata (Cloud Architect)',
      title: '.NET & Azure Microservices',
      skills: '.NET, C#, Azure, Microservices, Docker, SQL',
      isPrimary: false,
    },
  ]);

  readonly activeProfileId = signal<string>('profile-1');
  readonly profileChanged = output<ProfileOption>();
  readonly addProfileClick = output<void>();

  selectProfile(p: ProfileOption): void {
    this.activeProfileId.set(p.id);
    this.profileChanged.emit(p);
  }
}
