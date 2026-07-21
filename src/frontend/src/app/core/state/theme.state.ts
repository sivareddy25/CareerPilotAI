import { signal, computed } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

export const activeThemeMode = signal<ThemeMode>('system');
export const resolvedTheme = signal<'light' | 'dark'>('light');

export const isDarkMode = computed(() => resolvedTheme() === 'dark');
