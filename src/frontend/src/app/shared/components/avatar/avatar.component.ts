import { Component, ChangeDetectionStrategy, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
export type AvatarStatus = 'online' | 'offline' | 'busy' | 'away';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="avatarClasses()" [attr.aria-label]="alt() || name()">
      @if (src() && !imageError()) {
        <img [src]="src()" [alt]="alt() || name()" (error)="onImageError()" class="avatar-img" />
      } @else {
        <span class="avatar-initials">{{ initials() }}</span>
      }
      @if (status()) {
        <span [class]="statusClasses()" [attr.title]="status()"></span>
      }
    </div>
  `,
  styles: [`
    .avatar {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-circle);
      background-color: var(--brand-primary-alpha);
      color: var(--brand-primary);
      font-weight: 600;
      user-select: none;

      &-img {
        width: 100%;
        height: 100%;
        border-radius: var(--radius-circle);
        object-fit: cover;
      }

      &-xs { width: 24px; height: 24px; font-size: 10px; }
      &-sm { width: 32px; height: 32px; font-size: 12px; }
      &-md { width: 40px; height: 40px; font-size: 14px; }
      &-lg { width: 48px; height: 48px; font-size: 18px; }
      &-xl { width: 64px; height: 64px; font-size: 24px; }

      .status-dot {
        position: absolute;
        bottom: 0;
        right: 0;
        width: 25%;
        height: 25%;
        min-width: 8px;
        min-height: 8px;
        border-radius: var(--radius-circle);
        border: 2px solid var(--bg-primary);

        &-online { background-color: var(--success); }
        &-offline { background-color: var(--text-muted); }
        &-busy { background-color: var(--danger); }
        &-away { background-color: var(--warning); }
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarComponent {
  readonly src = input<string>();
  readonly name = input<string>('User');
  readonly alt = input<string>();
  readonly size = input<AvatarSize>('md');
  readonly status = input<AvatarStatus>();

  protected imageError = signal<boolean>(false);

  protected initials = computed(() => {
    const nameStr = this.name().trim();
    if (!nameStr) return 'U';
    const parts = nameStr.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return nameStr.substring(0, 2).toUpperCase();
  });

  protected avatarClasses = computed(() => {
    return `avatar avatar-${this.size()}`;
  });

  protected statusClasses = computed(() => {
    return `status-dot status-dot-${this.status()}`;
  });

  protected onImageError(): void {
    this.imageError.set(true);
  }
}
