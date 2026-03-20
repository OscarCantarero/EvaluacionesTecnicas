import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorStateService } from './error-state.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorState = inject(ErrorStateService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        errorState.setFromHttpError(error);
      }

      return throwError(() => error);
    })
  );
};
