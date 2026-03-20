import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { SesionesApiService } from '../../core/sesiones/sesiones-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';

@Component({
  selector: 'app-candidate-access-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent, RouterLink],
  templateUrl: './candidate-access-page.component.html',
  styleUrl: './candidate-access-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CandidateAccessPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly sesionesApi = inject(SesionesApiService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    sesionId: ['', [Validators.required]],
    codigoAcceso: ['', [Validators.required, Validators.minLength(8)]]
  });

  private readonly formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });
  protected readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.isSubmitting());

  protected iniciar(): void {
    this.form.markAllAsTouched();
    if (!this.form.valid || this.isSubmitting()) {
      return;
    }

    const { sesionId, codigoAcceso } = this.form.getRawValue();
    this.isSubmitting.set(true);

    this.sesionesApi
      .iniciarSesion(sesionId, codigoAcceso)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigate(['/candidato/cuestionario', sesionId], {
            queryParams: { codigo: codigoAcceso }
          });
        }
      });
  }
}
