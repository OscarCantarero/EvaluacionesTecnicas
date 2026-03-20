import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/auth/auth.models';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';

@Component({
  selector: 'app-user-admin-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent],
  templateUrl: './user-admin-page.component.html',
  styleUrl: './user-admin-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserAdminPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  protected readonly isSubmitting = signal(false);
  protected readonly createdUserId = signal<string | null>(null);
  protected readonly roles: ReadonlyArray<UserRole> = ['Administrador', 'Evaluador', 'Candidato'];

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/[0-9]/)
      ]
    ],
    rol: ['Evaluador' as UserRole]
  });

  private readonly formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });
  protected readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.isSubmitting());

  protected crearUsuario(): void {
    this.form.markAllAsTouched();
    if (!this.form.valid || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.createdUserId.set(null);

    const value = this.form.getRawValue();
    this.authService
      .registrarUsuario({
        email: value.email.trim(),
        nombre: value.nombre.trim(),
        password: value.password,
        rol: value.rol
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.createdUserId.set(response.id);
          this.form.reset({
            email: '',
            nombre: '',
            password: '',
            rol: 'Evaluador'
          });
        }
      });
  }
}
