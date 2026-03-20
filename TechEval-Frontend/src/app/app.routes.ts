import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth/auth.guard';
import { AppShellComponent } from './layout/app-shell.component';
import { CandidateAccessPageComponent } from './pages/candidate-access/candidate-access-page.component';
import { CandidateQuizPageComponent } from './pages/candidate-quiz/candidate-quiz-page.component';
import { CandidateScoresPageComponent } from './pages/candidate-scores/candidate-scores-page.component';
import { CategoriasPageComponent } from './pages/categorias/categorias-page.component';
import { ComparePageComponent } from './pages/compare/compare-page.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';
import { FormCreatePageComponent } from './pages/forms/form-create-page.component';
import { FormDetailPageComponent } from './pages/forms/form-detail-page.component';
import { FormsPageComponent } from './pages/forms/forms-page.component';
import { LoginPageComponent } from './pages/login/login-page.component';
import { ResultsPageComponent } from './pages/results/results-page.component';
import { SessionsPageComponent } from './pages/sessions/sessions-page.component';
import { UserAdminPageComponent } from './pages/user-admin/user-admin-page.component';

export const routes: Routes = [
	{ path: 'login', component: LoginPageComponent },
	{ path: 'candidato/acceso', component: CandidateAccessPageComponent, canActivate: [authGuard] },
	{ path: 'candidato/cuestionario/:sesionId', component: CandidateQuizPageComponent },
	{
		path: '',
		component: AppShellComponent,
		canActivate: [authGuard],
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
			{ path: 'dashboard', component: DashboardPageComponent },
			{
				path: 'mis-puntajes',
				component: CandidateScoresPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Candidato'] }
			},
			{
				path: 'formularios',
				component: FormsPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'formularios/nuevo',
				component: FormCreatePageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'formularios/:id',
				component: FormDetailPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'categorias',
				component: CategoriasPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'sesiones',
				component: SessionsPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'resultados',
				component: ResultsPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'comparar',
				component: ComparePageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Evaluador', 'Administrador'] }
			},
			{
				path: 'usuarios',
				component: UserAdminPageComponent,
				canActivate: [roleGuard],
				data: { roles: ['Administrador'] }
			}
		]
	},
	{
		path: '**',
		redirectTo: 'dashboard'
	}
];
