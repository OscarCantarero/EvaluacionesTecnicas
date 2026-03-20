import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { EvaluacionesApiService } from '../../core/evaluaciones/evaluaciones-api.service';
import { EvaluacionResumenDto } from '../../core/evaluaciones/evaluaciones.models';
import { ProblemBannerComponent } from '../../shared/problem-banner/problem-banner.component';
import { LabelPipe } from '../../shared/pipes/label.pipe';

@Component({
  selector: 'app-forms-page',
  imports: [DatePipe, FormsModule, ProblemBannerComponent, LabelPipe],
  templateUrl: './forms-page.component.html',
  styleUrl: './forms-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormsPageComponent implements OnInit {
  private readonly evaluacionesApi = inject(EvaluacionesApiService);
  private readonly router = inject(Router);

  protected readonly evaluaciones = signal<ReadonlyArray<EvaluacionResumenDto>>([]);
  protected readonly loading = signal(false);
  protected readonly pagina = signal(1);
  protected readonly totalPaginas = signal(0);
  protected readonly totalItems = signal(0);
  protected busqueda = '';
  protected estadoFiltro = '';

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.loading.set(true);
    this.evaluacionesApi
      .listarEvaluaciones(false, this.pagina(), 10, this.busqueda || undefined, this.estadoFiltro || undefined)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.evaluaciones.set(res?.items ?? []);
          this.totalPaginas.set(res?.totalPaginas ?? 0);
          this.totalItems.set(res?.totalItems ?? 0);
        }
      });
  }

  protected buscar(): void {
    this.pagina.set(1);
    this.cargar();
  }

  protected irPagina(p: number): void {
    this.pagina.set(p);
    this.cargar();
  }

  protected abrir(id: string): void {
    void this.router.navigate(['/formularios', id]);
  }

  protected nuevo(): void {
    void this.router.navigate(['/formularios', 'nuevo']);
  }
}
