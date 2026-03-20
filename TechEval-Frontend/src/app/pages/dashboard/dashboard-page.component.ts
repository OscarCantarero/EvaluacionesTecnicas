import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { ICONS } from '../../shared/icons/icons';
import { SafeHtmlPipe } from '../../shared/pipes/safe-html.pipe';

interface StatCard {
  readonly label: string;
  readonly value: string;
  readonly note: string;
  readonly icon: string;
}

@Component({
  selector: 'app-dashboard-page',
  imports: [SafeHtmlPipe],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent {
  protected readonly icons = ICONS;

  private readonly stats = signal<ReadonlyArray<StatCard>>([
    { label: 'Sesiones activas', value: '12', note: '+4 hoy', icon: ICONS.play },
    { label: 'Evaluaciones en revisión', value: '8', note: 'Revisión activa', icon: ICONS.star },
    { label: 'PDF generados', value: '34', note: 'Últimos 7 días', icon: ICONS.document }
  ]);

  protected readonly statCards = computed(() => this.stats());
}
