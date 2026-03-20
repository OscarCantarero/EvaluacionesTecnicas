import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CONFIG } from '../config/api-config';

export interface CategoriaDto {
  readonly id: string;
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly creadoEn: string;
}

export interface CrearCategoriaRequest {
  readonly nombre: string;
  readonly descripcion: string | null;
}

@Injectable({ providedIn: 'root' })
export class CategoriasApiService {
  private readonly http = inject(HttpClient);

  listar() {
    return this.http.get<CategoriaDto[]>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.categorias}`);
  }

  crear(payload: CrearCategoriaRequest) {
    return this.http.post<{ id: string }>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.categorias}`, payload);
  }

  actualizar(id: string, payload: CrearCategoriaRequest) {
    return this.http.put<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.categorias}/${id}`, payload);
  }

  eliminar(id: string) {
    return this.http.delete<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.categorias}/${id}`);
  }
}
