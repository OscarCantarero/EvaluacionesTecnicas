import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { PreguntaActualDto, PreguntaSesionBackendDto } from '../../core/sesiones/sesiones.models';
import { SesionesApiService } from '../../core/sesiones/sesiones-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';

@Component({
  selector: 'app-candidate-quiz-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent, RouterLink],
  templateUrl: './candidate-quiz-page.component.html',
  styleUrl: './candidate-quiz-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CandidateQuizPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly sesionesApi = inject(SesionesApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly sesionCompletada = signal(false);
  protected readonly preguntaActual = signal<PreguntaActualDto | null>(null);
  protected readonly sesionId = signal('');
  protected readonly progreso = signal({ respondidas: 0, total: 0 });
  protected readonly tiempoRestante = signal<number | null>(null);

  protected readonly adjunto = signal<File | null>(null);
  protected readonly adjuntoUrl = signal<string | null>(null);
  protected readonly uploadingAdjunto = signal(false);
  protected readonly errorSesion = signal<string | null>(null);

  // Selección para preguntas de opción
  protected readonly opcionSeleccionada = signal<string>('');          // SeleccionUnica
  protected readonly opcionesSeleccionadas = signal<Set<string>>(new Set()); // SeleccionMultiple

  protected readonly respuestaForm = this.fb.nonNullable.group({
    texto: ['']
  });

  protected readonly canSubmitRespuesta = computed(() => {
    if (this.submitting()) return false;
    const tipo = this.preguntaActual()?.tipo ?? '';
    if (tipo === 'SeleccionUnica')    return !!this.opcionSeleccionada();
    if (tipo === 'SeleccionMultiple') return this.opcionesSeleccionadas().size > 0;
    return this.respuestaForm.getRawValue().texto.trim().length > 0;
  });

  private timerInterval: ReturnType<typeof setInterval> | null = null;
  private tiempoInicioPreguntas = 0;

  constructor() {
    const sesionId = this.route.snapshot.paramMap.get('sesionId') ?? '';
    const codigo = this.route.snapshot.queryParamMap.get('codigo') ?? '';
    this.sesionId.set(sesionId);

    if (sesionId && codigo) {
      this.cargarSesion(sesionId, codigo);
    }

    document.addEventListener('visibilitychange', this.onVisibilityChange);
    this.destroyRef.onDestroy(() => {
      document.removeEventListener('visibilitychange', this.onVisibilityChange);
      this.pararTimer();
    });
  }

  protected enviarRespuesta(fueExpirado = false): void {
    const current = this.preguntaActual();
    if (!current || this.submitting()) return;
    if (!fueExpirado && !this.canSubmitRespuesta()) return;

    this.pararTimer();
    const tiempoEmpleado = Math.max(1, Math.floor((performance.now() - this.tiempoInicioPreguntas) / 1000));
    this.submitting.set(true);

    this.sesionesApi
      .registrarRespuesta(this.sesionId(), {
        preguntaId: current.preguntaId,
        texto: fueExpirado ? '' : this.getRespuestaTexto(current.tipo),
        tiempoEmpleadoSegundos: tiempoEmpleado,
        fueExpirado
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.progreso.set({ respondidas: response.preguntasRespondidas, total: response.totalPreguntas });
          this.respuestaForm.reset();
          this.adjunto.set(null);
          this.adjuntoUrl.set(null);

          if (response.sesionCompletada) {
            this.sesionCompletada.set(true);
            this.preguntaActual.set(null);
            return;
          }

          if (response.preguntaSiguiente) {
            this.mostrarPregunta(response.preguntaSiguiente);
          }
        },
        error: () => {
          this.errorSesion.set('Error al enviar la respuesta. Intenta de nuevo.');
        }
      });
  }

  protected omitirPregunta(): void {
    this.enviarRespuesta(true);
  }

  protected toggleOpcion(id: string): void {
    const set = new Set(this.opcionesSeleccionadas());
    if (set.has(id)) set.delete(id); else set.add(id);
    this.opcionesSeleccionadas.set(set);
  }

  protected isOpcionSeleccionada(id: string): boolean {
    return this.opcionesSeleccionadas().has(id);
  }

  private getRespuestaTexto(tipo: string): string {
    if (tipo === 'SeleccionUnica')    return this.opcionSeleccionada();
    if (tipo === 'SeleccionMultiple') return Array.from(this.opcionesSeleccionadas()).join(',');
    return this.respuestaForm.getRawValue().texto;
  }

  private cargarSesion(sesionId: string, codigo: string): void {
    this.loading.set(true);
    this.errorSesion.set(null);

    this.sesionesApi
      .iniciarSesion(sesionId, codigo)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.progreso.set({ respondidas: response.preguntasRespondidas, total: response.totalPreguntas });
          if (response.preguntasRespondidas > 0 && response.preguntasRespondidas === response.totalPreguntas) {
            this.sesionCompletada.set(true);
            return;
          }
          if (response.preguntaActual) {
            this.mostrarPregunta(response.preguntaActual);
          }
        },
        error: () => {
          this.errorSesion.set('No se pudo iniciar la sesión. Verifica el código de acceso e intenta de nuevo.');
        }
      });
  }

  private mostrarPregunta(dto: PreguntaSesionBackendDto): void {
    this.preguntaActual.set({
      preguntaId: dto.preguntaId,
      numero: dto.orden,
      tipo: dto.tipoPregunta,
      texto: dto.texto,
      dificultad: '',
      tiempoMaximoSegundos: dto.limiteTiempoSegundos,
      opciones: dto.opciones as { id: string; texto: string }[],
      permiteAdjunto: (dto as any).permiteAdjunto ?? false
    });
    this.opcionSeleccionada.set('');
    this.opcionesSeleccionadas.set(new Set());
    this.respuestaForm.reset();
    this.tiempoInicioPreguntas = performance.now();
    this.iniciarTimer(dto.limiteTiempoSegundos);
  }

  private iniciarTimer(segundos: number | null): void {
    this.pararTimer();
    if (!segundos) {
      this.tiempoRestante.set(null);
      return;
    }
    this.tiempoRestante.set(segundos);
    this.timerInterval = setInterval(() => {
      const actual = this.tiempoRestante();
      if (actual === null || actual <= 1) {
        this.pararTimer();
        this.tiempoRestante.set(0);
        this.enviarRespuesta(true);
      } else {
        this.tiempoRestante.set(actual - 1);
      }
    }, 1000);
  }

  private pararTimer(): void {
    if (this.timerInterval !== null) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  private readonly onVisibilityChange = (): void => {
    if (document.hidden && this.sesionId()) {
      this.sesionesApi.registrarViolacionPestana(this.sesionId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    }
  };

  protected onArchivoSeleccionado(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.adjunto.set(file);
    this.adjuntoUrl.set(null);
  }

  protected subirAdjunto(): void {
    const file = this.adjunto();
    const current = this.preguntaActual();
    if (!file || !current || this.uploadingAdjunto()) return;

    this.uploadingAdjunto.set(true);
    this.sesionesApi
      .subirAdjunto(this.sesionId(), current.preguntaId, file)
      .pipe(finalize(() => this.uploadingAdjunto.set(false)))
      .subscribe({
        next: (res) => this.adjuntoUrl.set(res.urlAdjunto)
      });
  }
}
