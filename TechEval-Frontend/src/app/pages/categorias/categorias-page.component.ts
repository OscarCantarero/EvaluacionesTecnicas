import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { CategoriasApiService, CategoriaDto } from '../../core/categorias/categorias-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';

@Component({
  selector: 'app-categorias-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent],
  templateUrl: './categorias-page.component.html',
  styleUrl: './categorias-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoriasPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly categoriasApi = inject(CategoriasApiService);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly categorias = signal<ReadonlyArray<CategoriaDto>>([]);
  protected readonly editId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    descripcion: ['', [Validators.maxLength(1000)]]
  });

  constructor() {
    this.cargar();
  }

  protected cargar(): void {
    this.loading.set(true);
    this.categoriasApi.listar().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (res) => this.categorias.set(res)
    });
  }

  protected iniciarEdicion(c: CategoriaDto): void {
    this.editId.set(c.id);
    this.form.setValue({ nombre: c.nombre, descripcion: c.descripcion ?? '' });
  }

  protected cancelarEdicion(): void {
    this.editId.set(null);
    this.form.reset({ nombre: '', descripcion: '' });
  }

  protected guardar(): void {
    if (!this.form.valid || this.saving()) return;
    const value = this.form.getRawValue();
    const payload = { nombre: value.nombre, descripcion: value.descripcion.trim() || null };
    this.saving.set(true);
    const id = this.editId();
    const onDone = () => {
      this.cancelarEdicion();
      this.cargar();
    };
    if (id) {
      this.categoriasApi.actualizar(id, payload)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({ next: onDone });
    } else {
      this.categoriasApi.crear(payload)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({ next: onDone });
    }
  }

  protected eliminar(id: string): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.categoriasApi.eliminar(id).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => this.cargar()
    });
  }
}
