import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'info' | 'warning' | 'error';

export interface ToastNotification {
  id: string;
  type: NotificationType;
  title: string;
  message?: string;
  durationMs?: number;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  readonly toasts = signal<ToastNotification[]>([]);

  show(type: NotificationType, title: string, message?: string, durationMs = 5000): string {
    const id = typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2);
    const toast: ToastNotification = { id, type, title, message, durationMs };

    this.toasts.update((items) => [...items, toast]);

    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }

    return id;
  }

  success(title: string, message?: string, durationMs?: number): string {
    return this.show('success', title, message, durationMs);
  }

  info(title: string, message?: string, durationMs?: number): string {
    return this.show('info', title, message, durationMs);
  }

  warning(title: string, message?: string, durationMs?: number): string {
    return this.show('warning', title, message, durationMs);
  }

  error(title: string, message?: string, durationMs?: number): string {
    return this.show('error', title, message, durationMs);
  }

  dismiss(id: string): void {
    this.toasts.update((items) => items.filter((t) => t.id !== id));
  }

  clearAll(): void {
    this.toasts.set([]);
  }
}
