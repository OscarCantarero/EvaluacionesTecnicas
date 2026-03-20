import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { finalize } from 'rxjs';
import { EvaluacionesApiService } from '../../core/evaluaciones/evaluaciones-api.service';
import {
  CompararCandidatosResponse,
  EvaluacionResumenDto,
  ObtenerRankingResponse
} from '../../core/evaluaciones/evaluaciones.models';
import { SesionesApiService } from '../../core/sesiones/sesiones-api.service';
import { SesionCandidatoDto } from '../../core/sesiones/sesiones.models';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';
import { ICONS } from '../../shared/icons/icons';
import { SafeHtmlPipe } from '../../shared/pipes/safe-html.pipe';

@Component({
  selector: 'app-compare-page',
  imports: [FormsModule, DecimalPipe, ProblemBannerComponent, LabelPipe, SafeHtmlPipe],
  templateUrl: './compare-page.component.html',
  styleUrl: './compare-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ComparePageComponent {
  private readonly evaluacionesApi = inject(EvaluacionesApiService);
  private readonly sesionesApi = inject(SesionesApiService);

  // Estado
  protected readonly evaluaciones = signal<ReadonlyArray<EvaluacionResumenDto>>([]);
  protected readonly sesiones = signal<ReadonlyArray<SesionCandidatoDto>>([]);
  protected readonly loading = signal(false);
  protected readonly loadingSesiones = signal(false);

  // Selección
  protected evaluacionSeleccionada = '';
  protected readonly sesionesSeleccionadas = signal<Set<string>>(new Set());

  // Resultados
  protected readonly comparacion = signal<CompararCandidatosResponse | null>(null);
  protected readonly ranking = signal<ObtenerRankingResponse | null>(null);
  protected readonly vistaActiva = signal<'comparar' | 'ranking'>('ranking');
  protected readonly icons = ICONS;

  constructor() {
    this.cargarEvaluaciones();
  }

  private cargarEvaluaciones(): void {
    this.evaluacionesApi.listarEvaluaciones(false, 1, 100).subscribe({
      next: (res) => this.evaluaciones.set(res?.items ?? [])
    });
  }

  protected onEvaluacionChange(): void {
    this.comparacion.set(null);
    this.ranking.set(null);
    this.sesionesSeleccionadas.set(new Set());
    if (!this.evaluacionSeleccionada) {
      this.sesiones.set([]);
      return;
    }
    this.loadingSesiones.set(true);
    this.sesionesApi
      .listarSesionesCandidato(1, 100, undefined, undefined)
      .pipe(finalize(() => this.loadingSesiones.set(false)))
      .subscribe({ next: (res) => this.sesiones.set(res?.items ?? []) });
  }

  protected toggleSesion(sesionId: string): void {
    const set = new Set(this.sesionesSeleccionadas());
    if (set.has(sesionId)) set.delete(sesionId); else set.add(sesionId);
    this.sesionesSeleccionadas.set(set);
  }

  protected seleccionarTodas(): void {
    this.sesionesSeleccionadas.set(new Set(this.sesiones().map(s => s.sesionId)));
  }

  protected deseleccionarTodas(): void {
    this.sesionesSeleccionadas.set(new Set());
  }

  protected cargarRanking(): void {
    if (!this.evaluacionSeleccionada || this.loading()) return;
    this.loading.set(true);
    this.vistaActiva.set('ranking');
    this.comparacion.set(null);

    this.evaluacionesApi
      .obtenerRanking(this.evaluacionSeleccionada)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({ next: (res) => this.ranking.set(res) });
  }

  protected compararSeleccionados(): void {
    const ids = [...this.sesionesSeleccionadas()];
    if (ids.length < 2 || !this.evaluacionSeleccionada || this.loading()) return;
    this.loading.set(true);
    this.vistaActiva.set('comparar');
    this.ranking.set(null);

    this.evaluacionesApi
      .compararCandidatos(this.evaluacionSeleccionada, { sesionIds: ids })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({ next: (res) => this.comparacion.set(res) });
  }

  protected formatTiempo(segundos: number): string {
    const min = Math.floor(segundos / 60);
    const sec = segundos % 60;
    return `${min}m ${sec}s`;
  }
}
