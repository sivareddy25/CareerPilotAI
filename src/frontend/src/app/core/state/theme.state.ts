import { signal, computed } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system' | 'high-contrast';

export const activeThemeMode = signal<ThemeMode>('system');
export const resolvedTheme = signal<'light' | 'dark' | 'high-contrast'>('light');

export const isDarkMode = computed(() => resolvedTheme() === 'dark');
export const isHighContrast = computed(() => resolvedTheme() === 'high-contrast');
