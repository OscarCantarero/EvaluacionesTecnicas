export type UserRole = 'Administrador' | 'Evaluador' | 'Candidato';

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

export interface LoginResponse {
  readonly accessToken: string;
  readonly refreshToken: string;
  readonly expiracion: string;
  readonly usuarioId: string;
  readonly email: string;
  readonly roles: ReadonlyArray<string>;
}

export interface RefreshResponse {
  readonly accessToken: string;
  readonly refreshToken: string;
  readonly expiracion: string;
}

export interface AuthSession {
  readonly accessToken: string;
  readonly refreshToken: string;
  readonly expiresAt: number;
  readonly userId: string;
  readonly name: string;
  readonly role: UserRole | null;
}

export interface RegistrarUsuarioRequest {
  readonly email: string;
  readonly nombre: string;
  readonly password: string;
  readonly rol: UserRole;
}

export interface RegistrarUsuarioResponse {
  readonly id: string;
}

export interface UsuarioResumenDto {
  readonly id: string;
  readonly email: string;
  readonly nombre: string;
  readonly rol: string;
}
