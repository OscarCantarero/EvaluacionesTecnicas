import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  readonly id: number;
  readonly message: string;
  readonly type: ToastType;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<ReadonlyArray<Toast>>([]);

  private idCounter = 0;

  showSuccess(message: string): void {
    this.add(message, 'success');
  }

  showError(message: string): void {
    this.add(message, 'error');
  }

  showInfo(message: string): void {
    this.add(message, 'info');
  }

  dismiss(id: number): void {
    this.toasts.update((current) => current.filter((t) => t.id !== id));
  }

  private add(message: string, type: ToastType): void {
    const id = ++this.idCounter;
    this.toasts.update((current) => [...current, { id, message, type }]);
    setTimeout(() => this.dismiss(id), 4000);
  }
}
