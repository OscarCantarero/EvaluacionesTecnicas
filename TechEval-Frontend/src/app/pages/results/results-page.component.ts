import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ResultadosApiService, ResultadoSesionDto, EvaluarIaResponse, CompletarRevisionResponse } from '../../core/resultados/resultados-api.service';
import { SesionesApiService } from '../../core/sesiones/sesiones-api.service';
import { SesionCandidatoDto } from '../../core/sesiones/sesiones.models';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';
import { ICONS } from '../../shared/icons/icons';
import { SafeHtmlPipe } from '../../shared/pipes/safe-html.pipe';

@Component({
  selector: 'app-results-page',
  imports: [ReactiveFormsModule, DatePipe, ProblemBannerComponent, LabelPipe, SafeHtmlPipe],
  templateUrl: './results-page.component.html',
  styleUrl: './results-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResultsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly sesionesApi = inject(SesionesApiService);
  private readonly resultadosApi = inject(ResultadosApiService);

  protected readonly loadingSesiones = signal(false);
  protected readonly loadingResultado = signal(false);
  protected readonly sesiones = signal<ReadonlyArray<SesionCandidatoDto>>([]);
  protected readonly resultado = signal<ResultadoSesionDto | null>(null);
  protected readonly sesionActiva = signal<string | null>(null);
  protected readonly pdfUrl = signal<string | null>(null);
  protected readonly iaResultado = signal<EvaluarIaResponse | null>(null);
  protected readonly loadingAccion = signal(false);
  protected readonly revisionMsg = signal<string | null>(null);
  protected readonly pagina = signal(1);
  protected readonly totalPaginas = signal(0);
  protected readonly totalItems = signal(0);
  protected readonly icons = ICONS;

  // Transcripciones
  protected readonly subiendoTranscripcion = signal(false);
  protected readonly transcripcionMsg = signal<string | null>(null);
  protected readonly transcripcionForm = this.fb.nonNullable.group({
    tipo: ['Entrevista'],
    contenido: ['']
  });
  protected readonly archivoTranscripcion = signal<File | null>(null);

  protected readonly puntajeForm = this.fb.nonNullable.group({
    preguntaId: ['', [Validators.required]],
    puntaje: [0, [Validators.required]],
    observaciones: ['']
  });

  protected readonly iaForm = this.fb.nonNullable.group({
    preguntaId: ['', [Validators.required]],
    contextoEvaluador: ['Eres un evaluador técnico senior especializado en el área. Evalúa la respuesta de forma objetiva.', [Validators.required]],
    escalaPuntuacion: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
    rubrica: ['']
  });

  protected readonly rechazoForm = this.fb.nonNullable.group({
    puntajeAlternativo: [0, [Validators.required]],
    motivoRechazo: ['', [Validators.required]]
  });

  constructor() {
    this.cargarSesiones();
  }

  protected cargarSesiones(): void {
    this.loadingSesiones.set(true);
    this.sesionesApi
      .listarSesionesCandidato(this.pagina(), 10)
      .pipe(finalize(() => this.loadingSesiones.set(false)))
      .subscribe({
        next: (res) => {
          this.sesiones.set(res?.items ?? []);
          this.totalPaginas.set(res?.totalPaginas ?? 0);
          this.totalItems.set(res?.totalItems ?? 0);
        }
      });
  }

  protected irPagina(p: number): void {
    this.pagina.set(p);
    this.cargarSesiones();
  }

  protected abrirResultado(sesionId: string): void {
    this.sesionActiva.set(sesionId);
    this.loadingResultado.set(true);
    this.resultadosApi
      .obtenerResultado(sesionId)
      .pipe(finalize(() => this.loadingResultado.set(false)))
      .subscribe({
        next: (res) => {
          this.resultado.set(res);
          const firstPreguntaId = res.puntuaciones[0]?.preguntaId ?? '';
          this.puntajeForm.controls.preguntaId.setValue(firstPreguntaId);
          this.iaForm.controls.preguntaId.setValue(firstPreguntaId);
          this.pdfUrl.set(null);
          this.iaResultado.set(null);
        }
      });
  }

  protected asignarPuntajeManual(): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || !this.puntajeForm.valid || this.loadingAccion()) {
      return;
    }

    const value = this.puntajeForm.getRawValue();
    this.loadingAccion.set(true);
    this.resultadosApi
      .asignarPuntuacion(sesionId, {
        preguntaId: value.preguntaId,
        puntaje: value.puntaje,
        observaciones: value.observaciones.trim() || null
      })
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({ next: () => this.abrirResultado(sesionId) });
  }

  protected evaluarConIa(): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || !this.iaForm.valid || this.loadingAccion()) {
      return;
    }

    const value = this.iaForm.getRawValue();
    this.loadingAccion.set(true);
    this.resultadosApi
      .evaluarConIa(sesionId, {
        preguntaId: value.preguntaId,
        contextoEvaluador: value.contextoEvaluador,
        escalaPuntuacion: value.escalaPuntuacion,
        rubrica: value.rubrica.trim() || null
      })
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({
        next: (res) => {
          this.iaResultado.set(res);
          this.abrirResultado(sesionId);
        }
      });
  }

  protected aceptarSugerenciaIa(): void {
    const sesionId = this.sesionActiva();
    const ia = this.iaResultado();
    if (!sesionId || !ia || this.loadingAccion()) return;

    this.loadingAccion.set(true);
    this.resultadosApi
      .aceptarSugerenciaIa(sesionId, {
        preguntaId: ia.preguntaId,
        puntajeAceptado: ia.puntajeSugerido
      })
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({
        next: () => {
          this.iaResultado.set(null);
          this.abrirResultado(sesionId);
        }
      });
  }

  protected rechazarSugerenciaIa(): void {
    const sesionId = this.sesionActiva();
    const ia = this.iaResultado();
    if (!sesionId || !ia || !this.rechazoForm.valid || this.loadingAccion()) return;

    const value = this.rechazoForm.getRawValue();
    this.loadingAccion.set(true);
    this.resultadosApi
      .rechazarSugerenciaIa(sesionId, {
        preguntaId: ia.preguntaId,
        puntajeManualAlternativo: value.puntajeAlternativo,
        motivoRechazo: value.motivoRechazo.trim() || null
      })
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({
        next: () => {
          this.iaResultado.set(null);
          this.abrirResultado(sesionId);
        }
      });
  }

  protected generarPdf(): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || this.loadingAccion()) {
      return;
    }

    this.loadingAccion.set(true);
    this.resultadosApi
      .generarPdf(sesionId)
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({ next: (res) => this.pdfUrl.set(res.urlPDF) });
  }

  protected completarRevision(): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || this.loadingAccion()) return;

    this.loadingAccion.set(true);
    this.revisionMsg.set(null);
    this.resultadosApi
      .completarRevision(sesionId)
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({
        next: (res) => {
          this.revisionMsg.set(res.mensaje);
          this.abrirResultado(sesionId);
        }
      });
  }

  protected onArchivoTranscripcion(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.archivoTranscripcion.set(input.files?.[0] ?? null);
  }

  protected subirTranscripcion(): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || this.subiendoTranscripcion()) return;
    const value = this.transcripcionForm.getRawValue();
    this.subiendoTranscripcion.set(true);
    this.resultadosApi
      .subirTranscripcion(sesionId, value.tipo, value.contenido.trim() || null, this.archivoTranscripcion())
      .pipe(finalize(() => this.subiendoTranscripcion.set(false)))
      .subscribe({
        next: () => {
          this.transcripcionMsg.set('Transcripción subida correctamente.');
          this.transcripcionForm.reset({ tipo: 'Entrevista', contenido: '' });
          this.archivoTranscripcion.set(null);
          this.abrirResultado(sesionId);
        }
      });
  }

  protected evaluarTranscripcionIa(transcripcionId: string): void {
    const sesionId = this.sesionActiva();
    if (!sesionId || this.loadingAccion()) return;
    this.loadingAccion.set(true);
    this.resultadosApi
      .evaluarTranscripcionConIa(
        sesionId,
        transcripcionId,
        'Eres un evaluador técnico senior. Evalúa la transcripción y da un puntaje de 0 a 100.'
      )
      .pipe(finalize(() => this.loadingAccion.set(false)))
      .subscribe({ next: () => this.abrirResultado(sesionId) });
  }
}
