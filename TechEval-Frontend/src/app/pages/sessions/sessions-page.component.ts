import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { UsuarioResumenDto } from '../../core/auth/auth.models';
import { EvaluacionesApiService } from '../../core/evaluaciones/evaluaciones-api.service';
import { EvaluacionResumenDto } from '../../core/evaluaciones/evaluaciones.models';
import { SesionesApiService, CrearSesionesMasivasResponse } from '../../core/sesiones/sesiones-api.service';
import { SesionCandidatoDto } from '../../core/sesiones/sesiones.models';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';
import { ICONS } from '../../shared/icons/icons';
import { SafeHtmlPipe } from '../../shared/pipes/safe-html.pipe';

@Component({
  selector: 'app-sessions-page',
  imports: [ReactiveFormsModule, FormsModule, DatePipe, ProblemBannerComponent, RouterLink, LabelPipe, SafeHtmlPipe],
  templateUrl: './sessions-page.component.html',
  styleUrl: './sessions-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SessionsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly sesionesApi = inject(SesionesApiService);
  private readonly evaluacionesApi = inject(EvaluacionesApiService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly loadingList = signal(false);
  protected readonly loadingOpciones = signal(false);
  protected readonly sesiones = signal<ReadonlyArray<SesionCandidatoDto>>([]);
  protected readonly evaluaciones = signal<ReadonlyArray<EvaluacionResumenDto>>([]);
  protected readonly candidatos = signal<ReadonlyArray<UsuarioResumenDto>>([]);
  protected readonly creationResult = signal<{
    readonly sesionId: string;
    readonly codigoAcceso: string;
  } | null>(null);
  protected readonly bulkResult = signal<CrearSesionesMasivasResponse | null>(null);
  protected readonly modoMasivo = signal(false);
  protected readonly icons = ICONS;

  // Paginación y filtros
  protected readonly pagina = signal(1);
  protected readonly totalPaginas = signal(0);
  protected readonly totalItems = signal(0);
  protected filtroEstado = '';
  protected filtroBusqueda = '';

  // Selección múltiple de candidatos
  protected readonly candidatosSeleccionados = signal<Set<string>>(new Set());

  protected readonly form = this.fb.nonNullable.group({
    evaluacionId: ['', [Validators.required]],
    candidatoId: ['', [Validators.required]]
  });

  private readonly formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });
  private readonly evaluacionIdValue = toSignal(this.form.controls.evaluacionId.valueChanges, { initialValue: this.form.controls.evaluacionId.value });
  protected readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.isSubmitting());

  protected readonly canSubmitBulk = computed(() =>
    this.evaluacionIdValue() !== '' &&
    this.candidatosSeleccionados().size > 0 &&
    !this.isSubmitting()
  );

  constructor() {
    this.cargarOpciones();
    this.cargarSesiones();
  }

  protected cargarOpciones(): void {
    this.loadingOpciones.set(true);
    forkJoin({
      evaluaciones: this.evaluacionesApi.listarEvaluaciones(false, 1, 100),
      candidatos: this.authService.listarUsuarios('Candidato')
    })
      .pipe(finalize(() => this.loadingOpciones.set(false)))
      .subscribe({
        next: ({ evaluaciones, candidatos }) => {
          this.evaluaciones.set(evaluaciones?.items ?? []);
          this.candidatos.set(candidatos ?? []);
        }
      });
  }

  protected toggleCandidato(id: string): void {
    const set = new Set(this.candidatosSeleccionados());
    if (set.has(id)) set.delete(id); else set.add(id);
    this.candidatosSeleccionados.set(set);
  }

  protected seleccionarTodos(): void {
    const all = new Set(this.candidatos().map(c => c.id));
    this.candidatosSeleccionados.set(all);
  }

  protected deseleccionarTodos(): void {
    this.candidatosSeleccionados.set(new Set());
  }

  protected crearSesion(): void {
    this.form.markAllAsTouched();
    if (!this.form.valid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.creationResult.set(null);

    this.sesionesApi
      .crearSesion(this.form.getRawValue())
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.creationResult.set({
            sesionId: response.sesionId,
            codigoAcceso: response.codigoAcceso
          });
          this.cargarSesiones();
        }
      });
  }

  protected crearSesionesMasivas(): void {
    const evalId = this.form.controls.evaluacionId.value;
    const candidatoIds = [...this.candidatosSeleccionados()];
    if (!evalId || candidatoIds.length === 0 || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.bulkResult.set(null);

    this.sesionesApi
      .crearSesionesMasivas({ evaluacionId: evalId, candidatoIds })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (res) => {
          this.bulkResult.set(res);
          this.deseleccionarTodos();
          this.cargarSesiones();
        }
      });
  }

  protected cargarSesiones(): void {
    this.loadingList.set(true);
    this.sesionesApi
      .listarSesionesCandidato(
        this.pagina(), 10,
        this.filtroEstado || undefined,
        this.filtroBusqueda || undefined
      )
      .pipe(finalize(() => this.loadingList.set(false)))
      .subscribe({
        next: (res) => {
          this.sesiones.set(res?.items ?? []);
          this.totalPaginas.set(res?.totalPaginas ?? 0);
          this.totalItems.set(res?.totalItems ?? 0);
        }
      });
  }

  protected filtrar(): void {
    this.pagina.set(1);
    this.cargarSesiones();
  }

  protected irPagina(p: number): void {
    this.pagina.set(p);
    this.cargarSesiones();
  }

  protected verResultado(sesionId: string): void {
    void this.router.navigate(['/resultados'], { queryParams: { sesionId } });
  }

  protected copiarTexto(texto: string): void {
    navigator.clipboard.writeText(texto);
  }
}
