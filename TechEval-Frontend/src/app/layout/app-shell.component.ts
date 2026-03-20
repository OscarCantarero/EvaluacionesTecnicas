import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../core/auth/auth.service';
import { ThemeService } from '../core/ui/theme.service';
import { ProblemBannerComponent } from '../shared/problem-banner/problem-banner.component';
import { ICONS } from '../shared/icons/icons';
import { SafeHtmlPipe } from '../shared/pipes/safe-html.pipe';

interface NavItem {
  readonly label: string;
  readonly route: string;
  readonly icon: string;
}

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, ProblemBannerComponent, SafeHtmlPipe],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'app-shell-host'
  }
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);

  protected readonly menuOpen = signal(false);
  protected readonly isLoggingOut = signal(false);
  protected readonly userName = computed(() => this.authService.userName());
  protected readonly userRole = computed(() => this.authService.userRole());
  protected readonly isDarkTheme = computed(() => this.themeService.isDark());

  protected readonly icons = ICONS;

  protected readonly allNavItems: ReadonlyArray<NavItem> = [
    { label: 'Panel general', route: '/dashboard', icon: ICONS.home },
    { label: 'Iniciar cuestionario', route: '/candidato/acceso', icon: ICONS.play },
    { label: 'Mis puntajes', route: '/mis-puntajes', icon: ICONS.star },
    { label: 'Formularios y preguntas', route: '/formularios', icon: ICONS.document },
    { label: 'Sesiones en vivo', route: '/sesiones', icon: ICONS.play },
    { label: 'Resultados y revisión', route: '/resultados', icon: ICONS.clipboard },
    { label: 'Comparar candidatos', route: '/comparar', icon: ICONS.chart },
    { label: 'Usuarios', route: '/usuarios', icon: ICONS.users }
  ];

  protected readonly navItems = computed(() => {
    const role = this.userRole();

    if (role === 'Candidato') {
      return this.allNavItems.filter(
        (item) => item.route === '/dashboard' || item.route === '/mis-puntajes' || item.route === '/candidato/acceso'
      );
    }

    if (role === 'Evaluador') {
      return this.allNavItems.filter((item) => item.route !== '/mis-puntajes' && item.route !== '/usuarios' && item.route !== '/candidato/acceso');
    }

    return this.allNavItems.filter((item) => item.route !== '/mis-puntajes');
  });

  protected toggleMenu(): void {
    this.menuOpen.update((current) => !current);
  }

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  protected logout(): void {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);
    this.authService
      .logout()
      .pipe(finalize(() => this.isLoggingOut.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigate(['/login']);
        }
      });
  }

  protected toggleTheme(): void {
    this.themeService.toggle();
  }
}
