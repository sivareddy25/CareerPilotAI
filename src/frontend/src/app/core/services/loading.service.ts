import { Injectable } from '@angular/core';
import { activeRequestCount, globalLoadingSignal } from '../state/loading.state';

@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  readonly isLoading = globalLoadingSignal;

  show(): void {
    activeRequestCount.update((count) => count + 1);
  }

  hide(): void {
    activeRequestCount.update((count) => Math.max(0, count - 1));
  }

  reset(): void {
    activeRequestCount.set(0);
  }
}
