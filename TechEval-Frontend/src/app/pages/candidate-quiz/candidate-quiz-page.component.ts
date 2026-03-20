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

  // Tab Lock — violaciones de pestaña
  protected readonly mostrarModalViolacion = signal(false);
  protected readonly contadorViolaciones = signal(0);

  // Screen Recording
  protected readonly grabandoPantalla = signal(false);
  protected readonly grabacionSesionBlob = signal<Blob | null>(null);
  protected readonly subiendoGrabacion = signal(false);
  private mediaRecorderPantalla: MediaRecorder | null = null;
  private chunksPantalla: BlobPart[] = [];

  // Audio Recording
  protected readonly grabandoAudio = signal(false);
  protected readonly grabacionAudioBlob = signal<Blob | null>(null);
  protected readonly subiendoAudio = signal(false);
  private mediaRecorderAudio: MediaRecorder | null = null;
  private chunksAudio: BlobPart[] = [];

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

  private readonly onBeforeUnload = (e: BeforeUnloadEvent): void => {
    if (!this.sesionCompletada()) {
      e.preventDefault();
    }
  };

  constructor() {
    const sesionId = this.route.snapshot.paramMap.get('sesionId') ?? '';
    const codigo = this.route.snapshot.queryParamMap.get('codigo') ?? '';
    this.sesionId.set(sesionId);

    if (sesionId && codigo) {
      this.cargarSesion(sesionId, codigo);
    }

    document.addEventListener('visibilitychange', this.onVisibilityChange);
    window.addEventListener('blur', this.onVisibilityChange);
    window.addEventListener('beforeunload', this.onBeforeUnload);

    this.destroyRef.onDestroy(() => {
      document.removeEventListener('visibilitychange', this.onVisibilityChange);
      window.removeEventListener('blur', this.onVisibilityChange);
      window.removeEventListener('beforeunload', this.onBeforeUnload);
      this.pararTimer();
      this.detenerGrabacionPantalla();
      this.detenerGrabacionAudio();
    });
  }

  protected cerrarModalViolacion(): void {
    this.mostrarModalViolacion.set(false);
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
            this.detenerGrabacionPantalla();
            this.detenerGrabacionAudio();
            this.subirGrabaciones();
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
          this.iniciarGrabacionPantalla().catch(err => console.warn('No se pudo iniciar grabación de pantalla:', err));
          this.iniciarGrabacionAudio().catch(err => console.warn('No se pudo iniciar grabación de audio:', err));
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
    if (document.hidden && this.sesionId() && !this.sesionCompletada()) {
      this.sesionesApi.registrarViolacionPestana(this.sesionId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
      this.contadorViolaciones.update(n => n + 1);
      this.mostrarModalViolacion.set(true);
    }
  };

  // ── Screen Recording ──────────────────────────────────────────────────────

  private async iniciarGrabacionPantalla(): Promise<void> {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
      this.chunksPantalla = [];
      this.mediaRecorderPantalla = new MediaRecorder(stream);
      this.mediaRecorderPantalla.ondataavailable = (e) => {
        if (e.data.size > 0) this.chunksPantalla.push(e.data);
      };
      this.mediaRecorderPantalla.onstop = () => {
        const blob = new Blob(this.chunksPantalla, { type: 'video/webm' });
        this.grabacionSesionBlob.set(blob);
        stream.getTracks().forEach(t => t.stop());
      };
      this.mediaRecorderPantalla.start();
      this.grabandoPantalla.set(true);
    } catch (err) {
      console.warn('Grabación de pantalla no disponible o denegada:', err);
    }
  }

  private detenerGrabacionPantalla(): void {
    if (this.mediaRecorderPantalla && this.mediaRecorderPantalla.state !== 'inactive') {
      this.mediaRecorderPantalla.stop();
      this.grabandoPantalla.set(false);
    }
  }

  // ── Audio Recording ───────────────────────────────────────────────────────

  private async iniciarGrabacionAudio(): Promise<void> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.chunksAudio = [];
      this.mediaRecorderAudio = new MediaRecorder(stream);
      this.mediaRecorderAudio.ondataavailable = (e) => {
        if (e.data.size > 0) this.chunksAudio.push(e.data);
      };
      this.mediaRecorderAudio.onstop = () => {
        const blob = new Blob(this.chunksAudio, { type: 'audio/webm' });
        this.grabacionAudioBlob.set(blob);
        stream.getTracks().forEach(t => t.stop());
      };
      this.mediaRecorderAudio.start();
      this.grabandoAudio.set(true);
    } catch (err) {
      console.warn('Grabación de audio no disponible o denegada:', err);
    }
  }

  private detenerGrabacionAudio(): void {
    if (this.mediaRecorderAudio && this.mediaRecorderAudio.state !== 'inactive') {
      this.mediaRecorderAudio.stop();
      this.grabandoAudio.set(false);
    }
  }

  // ── Upload recordings on completion ──────────────────────────────────────

  private subirGrabaciones(): void {
    const sesionBlob = this.grabacionSesionBlob();
    if (sesionBlob) {
      this.sesionesApi.subirGrabacion(this.sesionId(), sesionBlob)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe();
    }
    const audioBlob = this.grabacionAudioBlob();
    if (audioBlob) {
      this.sesionesApi.subirAudio(this.sesionId(), audioBlob)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe();
    }
  }

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
