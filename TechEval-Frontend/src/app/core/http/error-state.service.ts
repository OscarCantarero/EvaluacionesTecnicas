import { Injectable, computed, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from './problem-details';

@Injectable({ providedIn: 'root' })
export class ErrorStateService {
  private readonly lastProblemState = signal<ProblemDetails | null>(null);

  readonly lastProblem = computed(() => this.lastProblemState());

  setFromHttpError(error: HttpErrorResponse): void {
    const payload = error.error as ProblemDetails | null;
    if (payload && typeof payload === 'object') {
      this.lastProblemState.set({
        ...payload,
        estado: payload.estado ?? payload.status ?? error.status,
        detalles: payload.detalles ?? payload.detalle ?? error.message
      });
      return;
    }

    this.lastProblemState.set({
      estado: error.status,
      titulo: 'Error inesperado',
      detalles: error.message
    });
  }

  clear(): void {
    this.lastProblemState.set(null);
  }
}
