import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { EvaluacionesApiService } from '../../core/evaluaciones/evaluaciones-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';

@Component({
  selector: 'app-form-create-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent],
  templateUrl: './form-create-page.component.html',
  styleUrl: './form-create-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormCreatePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly evaluacionesApi = inject(EvaluacionesApiService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    descripcion: ['', [Validators.maxLength(1000)]],
    ordenAleatorio: [false],
    ordenPorDificultad: [false]
  });

  protected get ordenIncompatible(): boolean {
    return this.form.controls.ordenAleatorio.value && this.form.controls.ordenPorDificultad.value;
  }

  protected cancelar(): void {
    void this.router.navigate(['/formularios']);
  }

  protected guardar(): void {
    this.form.markAllAsTouched();
    if (!this.form.valid || this.submitting() || this.ordenIncompatible) {
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);

    this.evaluacionesApi
      .crearEvaluacion({
        nombre: value.nombre.trim(),
        descripcion: value.descripcion.trim() || null,
        ordenAleatorio: value.ordenAleatorio,
        ordenPorDificultad: value.ordenPorDificultad
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          void this.router.navigate(['/formularios', response.id]);
        }
      });
  }
}
