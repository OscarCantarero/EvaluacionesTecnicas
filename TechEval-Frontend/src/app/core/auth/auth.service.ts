import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, map, of, tap } from 'rxjs';
import { API_CONFIG } from '../config/api-config';
import { AuthStorageService } from './auth-storage.service';
import {
  AuthSession,
  LoginRequest,
  LoginResponse,
  RefreshResponse,
  RegistrarUsuarioRequest,
  RegistrarUsuarioResponse,
  UsuarioResumenDto,
  UserRole
} from './auth.models';

interface RefreshRequest {
  readonly refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(AuthStorageService);

  private readonly sessionState = signal<AuthSession | null>(this.storage.load());

  readonly session = computed(() => this.sessionState());
  readonly isAuthenticated = computed(() => {
    const current = this.sessionState();
    return Boolean(current?.accessToken && (current.expiresAt ?? 0) > Date.now());
  });
  readonly userName = computed(() => this.sessionState()?.name ?? '');
  readonly userRole = computed(() => this.sessionState()?.role ?? null);

  login(payload: LoginRequest) {
    return this.http.post<LoginResponse>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.auth}/login`, payload).pipe(
      map((response) => this.toSession(response)),
      tap((session) => this.setSession(session))
    );
  }

  refresh() {
    const current = this.sessionState();
    if (!current?.refreshToken) {
      return of(null);
    }

    return this.http
      .post<RefreshResponse>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.auth}/refresh`, {
        refreshToken: current.refreshToken
      } satisfies RefreshRequest)
      .pipe(
        map((response) => ({
          ...current,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          expiresAt: this.parseExpiration(response.expiracion)
        })),
        tap((updated) => this.setSession(updated)),
        catchError(() => {
          this.clearSession();
          return of(null);
        })
      );
  }

  logout() {
    const refreshToken = this.sessionState()?.refreshToken ?? '';
    return this.http.post<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.auth}/logout`, { refreshToken }).pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession())
    );
  }

  registrarUsuario(payload: RegistrarUsuarioRequest) {
    return this.http.post<RegistrarUsuarioResponse>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.auth}/registrar`, payload);
  }

  listarUsuarios(rol?: string) {
    const params = rol ? `?rol=${encodeURIComponent(rol)}` : '';
    return this.http.get<ReadonlyArray<UsuarioResumenDto>>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.auth}/usuarios${params}`);
  }

  hasAnyRole(roles: ReadonlyArray<UserRole>): boolean {
    const role = this.userRole();
    return role !== null && roles.includes(role);
  }

  getAccessToken(): string | null {
    return this.sessionState()?.accessToken ?? null;
  }

  private toSession(response: LoginResponse): AuthSession {
    const role = this.toKnownRole(response.roles);

    return {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: this.parseExpiration(response.expiracion),
      userId: response.usuarioId,
      name: response.email,
      role
    };
  }

  private parseExpiration(expirationIso: string): number {
    const parsed = Date.parse(expirationIso);
    return Number.isNaN(parsed) ? Date.now() + 60 * 60 * 1000 : parsed;
  }

  private toKnownRole(roles: ReadonlyArray<string>): UserRole | null {
    if (roles.includes('Administrador')) {
      return 'Administrador';
    }

    if (roles.includes('Evaluador')) {
      return 'Evaluador';
    }

    if (roles.includes('Candidato')) {
      return 'Candidato';
    }

    return null;
  }

  private setSession(session: AuthSession): void {
    this.sessionState.set(session);
    this.storage.save(session);
  }

  private clearSession(): void {
    this.sessionState.set(null);
    this.storage.clear();
  }
}
