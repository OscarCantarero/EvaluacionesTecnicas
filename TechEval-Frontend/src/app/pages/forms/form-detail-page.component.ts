import { ChangeDetectionStrategy, Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { EvaluacionesApiService } from '../../core/evaluaciones/evaluaciones-api.service';
import { EvaluacionDetalleDto, NivelDificultad, PreguntaBancoDto, TipoPregunta } from '../../core/evaluaciones/evaluaciones.models';
import { CategoriasApiService, CategoriaDto } from '../../core/categorias/categorias-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';

@Component({
  selector: 'app-form-detail-page',
  imports: [ReactiveFormsModule, ProblemBannerComponent, LabelPipe],
  templateUrl: './form-detail-page.component.html',
  styleUrl: './form-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly evaluacionesApi = inject(EvaluacionesApiService);
  private readonly categoriasApi = inject(CategoriasApiService);

  protected readonly evaluacion = signal<EvaluacionDetalleDto | null>(null);
  protected readonly evaluacionId = signal('');
  protected readonly loading = signal(false);
  protected readonly creatingPregunta = signal(false);
  protected readonly creatingOpcion = signal(false);
  protected readonly updatingEvaluacion = signal(false);
  protected readonly updatingPregunta = signal(false);
  protected readonly updatingOpcion = signal(false);
  protected readonly activando = signal(false);
  protected readonly categorias = signal<ReadonlyArray<CategoriaDto>>([]);

  // Banco de preguntas
  protected readonly mostrarBanco = signal(false);
  protected readonly bancoCargando = signal(false);
  protected readonly bancoPreguntas = signal<ReadonlyArray<PreguntaBancoDto>>([]);
  protected readonly bancoSeleccionadas = signal<Set<string>>(new Set());
  protected readonly agregandoDelBanco = signal(false);
  protected readonly bancoCategoriaFiltro = signal('');
  protected readonly bancoDificultadFiltro = signal('');
  protected readonly bancoTipoFiltro = signal('');

  private readonly opcionFormRef = viewChild<ElementRef>('opcionFormRef');

  protected readonly tiposPregunta: ReadonlyArray<TipoPregunta> = ['TextoLibre', 'SeleccionUnica', 'SeleccionMultiple'];
  protected readonly nivelesDificultad: ReadonlyArray<NivelDificultad> = ['Facil', 'Medio', 'Dificil'];

  protected readonly tipoLabels: Record<string, string> = {
    TextoLibre: 'Texto libre',
    SeleccionUnica: 'Selección única',
    SeleccionMultiple: 'Selección múltiple'
  };
  protected readonly nivelLabels: Record<string, string> = {
    Facil: 'Fácil',
    Medio: 'Medio',
    Dificil: 'Difícil'
  };

  protected readonly preguntasConOpciones = computed(
    () => this.evaluacion()?.preguntas.filter((p) => p.tipoPregunta !== 'TextoLibre') ?? []
  );

  protected readonly preguntaForm = this.fb.nonNullable.group({
    texto: ['', [Validators.required, Validators.maxLength(2000)]],
    tipoPregunta: ['TextoLibre' as TipoPregunta],
    nivelDificultad: ['Medio' as NivelDificultad],
    limiteTiempoSegundos: [0],
    permiteAdjunto: [false],
    esRevisionManual: [true],
    categoriaId: ['' as string]
  });

  protected readonly opcionForm = this.fb.nonNullable.group({
    preguntaId: ['', [Validators.required]],
    texto: ['', [Validators.required, Validators.maxLength(1000)]],
    puntuacion: [0],
    esRevisionManual: [false]
  });

  protected readonly editEvaluacionForm = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    descripcion: ['', [Validators.maxLength(1000)]],
    ordenAleatorio: [false],
    ordenPorDificultad: [false]
  });

  protected readonly editPreguntaForm = this.fb.nonNullable.group({
    preguntaId: ['', [Validators.required]],
    texto: ['', [Validators.required, Validators.maxLength(2000)]],
    tipoPregunta: ['TextoLibre' as TipoPregunta],
    nivelDificultad: ['Medio' as NivelDificultad],
    limiteTiempoSegundos: [0],
    permiteAdjunto: [false],
    esRevisionManual: [true],
    categoriaId: ['' as string]
  });

  protected readonly editOpcionForm = this.fb.nonNullable.group({
    preguntaId: ['', [Validators.required]],
    opcionId: ['', [Validators.required]],
    texto: ['', [Validators.required, Validators.maxLength(1000)]],
    puntuacion: [0],
    esRevisionManual: [false]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.evaluacionId.set(id);
    this.categoriasApi.listar().subscribe({ next: (c) => this.categorias.set(c) });
    if (id) {
      this.cargar();
    }
  }

  protected volver(): void {
    void this.router.navigate(['/formularios']);
  }

  protected cargar(): void {
    const id = this.evaluacionId();
    if (!id) {
      return;
    }

    this.loading.set(true);
    this.evaluacionesApi
      .obtenerEvaluacion(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (ev) => {
          this.evaluacion.set(ev);
          const first = ev.preguntas.find((p) => p.tipoPregunta !== 'TextoLibre')?.id ?? '';
          this.opcionForm.controls.preguntaId.setValue(first);

          this.editEvaluacionForm.reset({
            nombre: ev.nombre,
            descripcion: ev.descripcion ?? '',
            ordenAleatorio: ev.ordenAleatorio,
            ordenPorDificultad: ev.ordenPorDificultad
          });

          const firstPregunta = ev.preguntas[0];
          if (firstPregunta) {
            this.editPreguntaForm.reset({
              preguntaId: firstPregunta.id,
              texto: firstPregunta.texto,
              tipoPregunta: firstPregunta.tipoPregunta,
              nivelDificultad: firstPregunta.nivelDificultad,
              limiteTiempoSegundos: firstPregunta.limiteTiempoSegundos ?? 0,
              permiteAdjunto: firstPregunta.permiteAdjunto,
              esRevisionManual: firstPregunta.esRevisionManual,
              categoriaId: firstPregunta.categoriaId ?? ''
            });

            const firstOpcion = firstPregunta.opciones[0];
            this.editOpcionForm.reset({
              preguntaId: firstPregunta.id,
              opcionId: firstOpcion?.id ?? '',
              texto: firstOpcion?.texto ?? '',
              puntuacion: firstOpcion?.puntuacion ?? 0,
              esRevisionManual: firstOpcion?.esRevisionManual ?? false
            });
          }
        }
      });
  }

  protected cargarPreguntaEdicion(preguntaId: string): void {
    const pregunta = this.evaluacion()?.preguntas.find((p) => p.id === preguntaId);
    if (!pregunta) {
      return;
    }

    this.editPreguntaForm.reset({
      preguntaId: pregunta.id,
      texto: pregunta.texto,
      tipoPregunta: pregunta.tipoPregunta,
      nivelDificultad: pregunta.nivelDificultad,
      limiteTiempoSegundos: pregunta.limiteTiempoSegundos ?? 0,
      permiteAdjunto: pregunta.permiteAdjunto,
      esRevisionManual: pregunta.esRevisionManual,
      categoriaId: pregunta.categoriaId ?? ''
    });
  }

  protected cargarOpcionEdicion(preguntaId: string, opcionId: string): void {
    const pregunta = this.evaluacion()?.preguntas.find((p) => p.id === preguntaId);
    const opcion = pregunta?.opciones.find((o) => o.id === opcionId);
    if (!pregunta || !opcion) {
      return;
    }

    this.editOpcionForm.reset({
      preguntaId,
      opcionId,
      texto: opcion.texto,
      puntuacion: opcion.puntuacion ?? 0,
      esRevisionManual: opcion.esRevisionManual
    });
  }

  protected onPreguntaOpcionCambio(preguntaId: string): void {
    const pregunta = this.evaluacion()?.preguntas.find((p) => p.id === preguntaId);
    const opcion = pregunta?.opciones[0];
    if (!pregunta || !opcion) {
      return;
    }

    this.cargarOpcionEdicion(pregunta.id, opcion.id);
  }

  protected onOpcionCambio(opcionId: string): void {
    const preguntaId = this.editOpcionForm.controls.preguntaId.value;
    if (!preguntaId || !opcionId) {
      return;
    }

    this.cargarOpcionEdicion(preguntaId, opcionId);
  }

  protected guardarEvaluacion(): void {
    this.editEvaluacionForm.markAllAsTouched();
    if (!this.editEvaluacionForm.valid || this.updatingEvaluacion()) {
      return;
    }

    const v = this.editEvaluacionForm.getRawValue();
    this.updatingEvaluacion.set(true);

    this.evaluacionesApi
      .actualizarEvaluacion(this.evaluacionId(), {
        nombre: v.nombre.trim(),
        descripcion: v.descripcion.trim() || null,
        ordenAleatorio: v.ordenAleatorio,
        ordenPorDificultad: v.ordenPorDificultad
      })
      .pipe(finalize(() => this.updatingEvaluacion.set(false)))
      .subscribe({ next: () => this.cargar() });
  }

  protected guardarPregunta(): void {
    this.editPreguntaForm.markAllAsTouched();
    if (!this.editPreguntaForm.valid || this.updatingPregunta()) {
      return;
    }

    const v = this.editPreguntaForm.getRawValue();
    this.updatingPregunta.set(true);

    this.evaluacionesApi
      .actualizarPregunta(this.evaluacionId(), v.preguntaId, {
        texto: v.texto.trim(),
        tipoPregunta: v.tipoPregunta,
        nivelDificultad: v.nivelDificultad,
        limiteTiempoSegundos: v.limiteTiempoSegundos > 0 ? v.limiteTiempoSegundos : null,
        permiteAdjunto: v.permiteAdjunto,
        esRevisionManual: v.esRevisionManual,
        categoriaId: v.categoriaId || null
      })
      .pipe(finalize(() => this.updatingPregunta.set(false)))
      .subscribe({ next: () => this.cargar() });
  }

  protected guardarOpcion(): void {
    this.editOpcionForm.markAllAsTouched();
    if (!this.editOpcionForm.valid || this.updatingOpcion()) {
      return;
    }

    const v = this.editOpcionForm.getRawValue();
    this.updatingOpcion.set(true);

    this.evaluacionesApi
      .actualizarOpcion(this.evaluacionId(), v.preguntaId, v.opcionId, {
        texto: v.texto.trim(),
        puntuacion: v.esRevisionManual ? null : v.puntuacion,
        esRevisionManual: v.esRevisionManual
      })
      .pipe(finalize(() => this.updatingOpcion.set(false)))
      .subscribe({ next: () => this.cargar() });
  }

  protected agregarPregunta(): void {
    this.preguntaForm.markAllAsTouched();
    if (!this.preguntaForm.valid || this.creatingPregunta()) {
      return;
    }

    const v = this.preguntaForm.getRawValue();
    this.creatingPregunta.set(true);

    this.evaluacionesApi
      .crearPregunta(this.evaluacionId(), {
        texto: v.texto.trim(),
        tipoPregunta: v.tipoPregunta,
        nivelDificultad: v.nivelDificultad,
        limiteTiempoSegundos: v.limiteTiempoSegundos > 0 ? v.limiteTiempoSegundos : null,
        permiteAdjunto: v.permiteAdjunto,
        esRevisionManual: v.esRevisionManual,
        categoriaId: v.categoriaId || null
      })
      .pipe(finalize(() => this.creatingPregunta.set(false)))
      .subscribe({
        next: () => {
          this.preguntaForm.reset({
            texto: '',
            tipoPregunta: 'TextoLibre',
            nivelDificultad: 'Medio',
            limiteTiempoSegundos: 0,
            permiteAdjunto: false,
            esRevisionManual: true,
            categoriaId: ''
          });
          this.cargar();
        }
      });
  }

  protected agregarOpcion(): void {
    this.opcionForm.markAllAsTouched();
    if (!this.opcionForm.valid || this.creatingOpcion()) {
      return;
    }

    const v = this.opcionForm.getRawValue();
    this.creatingOpcion.set(true);

    this.evaluacionesApi
      .crearOpcion(this.evaluacionId(), v.preguntaId, {
        texto: v.texto.trim(),
        puntuacion: v.esRevisionManual ? null : v.puntuacion,
        esRevisionManual: v.esRevisionManual
      })
      .pipe(finalize(() => this.creatingOpcion.set(false)))
      .subscribe({
        next: () => {
          this.opcionForm.controls.texto.setValue('');
          this.opcionForm.controls.puntuacion.setValue(0);
          this.opcionForm.controls.esRevisionManual.setValue(false);
          this.cargar();
        }
      });
  }

  protected irAgregarOpcion(preguntaId: string): void {
    this.opcionForm.controls.preguntaId.setValue(preguntaId);
    this.opcionForm.controls.texto.setValue('');
    this.opcionForm.controls.puntuacion.setValue(0);
    this.opcionForm.controls.esRevisionManual.setValue(false);
    const el = this.opcionFormRef()?.nativeElement;
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  protected eliminarOpcion(preguntaId: string, opcionId: string): void {
    this.evaluacionesApi
      .eliminarOpcion(this.evaluacionId(), preguntaId, opcionId)
      .subscribe({ next: () => this.cargar() });
  }

  protected eliminarPregunta(preguntaId: string): void {
    this.evaluacionesApi
      .eliminarPregunta(this.evaluacionId(), preguntaId)
      .subscribe({ next: () => this.cargar() });
  }

  protected activarEvaluacion(): void {
    if (this.activando()) return;
    this.activando.set(true);
    this.evaluacionesApi
      .activarEvaluacion(this.evaluacionId())
      .pipe(finalize(() => this.activando.set(false)))
      .subscribe({ next: () => this.cargar() });
  }

  protected abrirBanco(): void {
    this.mostrarBanco.set(true);
    this.bancoSeleccionadas.set(new Set());
    this.cargarBanco();
  }

  protected cargarBanco(): void {
    const evalId = this.evaluacionId();
    if (!evalId) return;
    this.bancoCargando.set(true);
    this.evaluacionesApi
      .obtenerPreguntasBanco(
        evalId,
        this.bancoCategoriaFiltro() || undefined,
        this.bancoDificultadFiltro() || undefined,
        this.bancoTipoFiltro() || undefined
      )
      .pipe(finalize(() => this.bancoCargando.set(false)))
      .subscribe({ next: (res) => this.bancoPreguntas.set(res) });
  }

  protected toggleSeleccionBanco(id: string): void {
    const set = new Set(this.bancoSeleccionadas());
    if (set.has(id)) set.delete(id);
    else set.add(id);
    this.bancoSeleccionadas.set(set);
  }

  protected confirmarBanco(): void {
    const evalId = this.evaluacionId();
    const ids = [...this.bancoSeleccionadas()];
    if (!evalId || ids.length === 0 || this.agregandoDelBanco()) return;
    this.agregandoDelBanco.set(true);
    this.evaluacionesApi
      .agregarDelBanco(evalId, ids)
      .pipe(finalize(() => this.agregandoDelBanco.set(false)))
      .subscribe({
        next: () => {
          this.mostrarBanco.set(false);
          this.cargar();
        }
      });
  }
}
