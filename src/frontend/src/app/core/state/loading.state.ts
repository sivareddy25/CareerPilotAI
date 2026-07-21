import { signal, computed } from '@angular/core';

export const activeRequestCount = signal<number>(0);
export const globalLoadingSignal = computed(() => activeRequestCount() > 0);
