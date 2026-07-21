import { signal } from '@angular/core';
import { AppState } from '../models/app-state.model';

export const appStateSignal = signal<AppState>({
  isSidebarOpen: true,
  activePageTitle: 'CareerPilot AI',
  isOnline: typeof navigator !== 'undefined' ? navigator.onLine : true,
});
