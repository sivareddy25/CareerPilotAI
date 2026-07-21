import { Injectable, signal, computed } from '@angular/core';

export type HostingMode = 'Local' | 'SaaS';

@Injectable({
  providedIn: 'root',
})
export class HostingService {
  readonly mode = signal<HostingMode>('Local');

  readonly isLocalMode = computed(() => this.mode() === 'Local');
  readonly isSaaSMode = computed(() => this.mode() === 'SaaS');

  setMode(newMode: HostingMode): void {
    this.mode.set(newMode);
  }
}
