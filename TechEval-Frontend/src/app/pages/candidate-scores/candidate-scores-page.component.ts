import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { ResultadosApiService, ResultadoSesionDto } from '../../core/resultados/resultados-api.service';
import { SesionCandidatoDto } from '../../core/sesiones/sesiones.models';
import { SesionesApiService } from '../../core/sesiones/sesiones-api.service';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';
import { ICONS } from '../../shared/icons/icons';
import { SafeHtmlPipe } from '../../shared/pipes/safe-html.pipe';

@Component({
  selector: 'app-candidate-scores-page',
  imports: [DatePipe, ProblemBannerComponent, LabelPipe, SafeHtmlPipe],
  templateUrl: './candidate-scores-page.component.html',
  styleUrl: './candidate-scores-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CandidateScoresPageComponent {
  private readonly sesionesApi = inject(SesionesApiService);
  private readonly resultadosApi = inject(ResultadosApiService);

  protected readonly loadingSessions = signal(false);
  protected readonly loadingResult = signal(false);
  protected readonly sesiones = signal<ReadonlyArray<SesionCandidatoDto>>([]);
  protected readonly selectedSessionId = signal<string | null>(null);
  protected readonly selectedResult = signal<ResultadoSesionDto | null>(null);
  protected readonly pagina = signal(1);
  protected readonly totalPaginas = signal(0);
  protected readonly totalItems = signal(0);

  protected readonly hasSessions = computed(() => this.sesiones().length > 0);
  protected readonly icons = ICONS;
  protected readonly Math = Math;

  constructor() {
    this.cargarSesiones();
  }

  protected verResultado(sesionId: string): void {
    this.selectedSessionId.set(sesionId);
    this.selectedResult.set(null);
    this.loadingResult.set(true);

    this.resultadosApi
      .obtenerResultado(sesionId)
      .pipe(finalize(() => this.loadingResult.set(false)))
      .subscribe({
        next: (result) => this.selectedResult.set(result)
      });
  }

  private cargarSesiones(): void {
    this.loadingSessions.set(true);

    this.sesionesApi
      .listarSesionesCandidato(this.pagina(), 10)
      .pipe(finalize(() => this.loadingSessions.set(false)))
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
}
