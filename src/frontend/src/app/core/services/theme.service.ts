import { Injectable, effect } from '@angular/core';
import { STORAGE_KEYS } from '../constants/storage.constants';
import { activeThemeMode, resolvedTheme, ThemeMode } from '../state/theme.state';
import { StorageUtils } from '../../shared/utils/storage.utils';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  readonly mode = activeThemeMode;
  readonly resolved = resolvedTheme;

  constructor() {
    this.initTheme();
    effect(() => {
      const currentMode = this.mode();
      this.applyTheme(currentMode);
    });
  }

  setTheme(mode: ThemeMode): void {
    this.mode.set(mode);
    StorageUtils.setItem(STORAGE_KEYS.THEME_PREFERENCE, mode);
  }

  private initTheme(): void {
    const saved = StorageUtils.getItem<ThemeMode>(STORAGE_KEYS.THEME_PREFERENCE);
    if (saved && ['light', 'dark', 'system', 'high-contrast'].includes(saved)) {
      this.mode.set(saved);
    } else {
      this.mode.set('system');
    }

    if (typeof window !== 'undefined' && window.matchMedia) {
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (this.mode() === 'system') {
          this.applyTheme('system');
        }
      });
    }
  }

  private applyTheme(mode: ThemeMode): void {
    if (typeof document === 'undefined') return;

    let targetTheme: 'light' | 'dark' | 'high-contrast' = 'light';
    if (mode === 'system') {
      targetTheme =
        typeof window !== 'undefined' &&
        window.matchMedia &&
        window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light';
    } else {
      targetTheme = mode;
    }

    this.resolved.set(targetTheme);
    document.documentElement.setAttribute('data-theme', targetTheme);
  }
}
