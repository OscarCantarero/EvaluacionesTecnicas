import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CONFIG } from '../config/api-config';
import { ResultadoPaginado } from '../common/pagination.models';
import { IniciarSesionResponse, RegistrarRespuestaResponse, SesionCandidatoDto } from './sesiones.models';

export interface CrearSesionRequest {
  readonly evaluacionId: string;
  readonly candidatoId: string;
}

export interface CrearSesionesMasivasRequest {
  readonly evaluacionId: string;
  readonly candidatoIds: ReadonlyArray<string>;
}

export interface CrearSesionesMasivasResponse {
  readonly totalCreadas: number;
  readonly sesiones: ReadonlyArray<{
    readonly sesionId: string;
    readonly candidatoId: string;
    readonly codigoAcceso: string;
  }>;
}

export interface CrearSesionResponse {
  readonly sesionId: string;
  readonly codigoAcceso: string;
  readonly fechaCreacion: string;
  readonly mensaje: string;
}

interface IniciarSesionRequest {
  readonly codigoAcceso: string;
}

interface RegistrarRespuestaRequest {
  readonly preguntaId: string;
  readonly texto: string;
  readonly tiempoEmpleadoSegundos: number;
  readonly fueExpirado: boolean;
}

@Injectable({ providedIn: 'root' })
export class SesionesApiService {
  private readonly http = inject(HttpClient);

  crearSesion(payload: CrearSesionRequest) {
    return this.http.post<CrearSesionResponse>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}`, payload);
  }

  iniciarSesion(sesionId: string, codigoAcceso: string) {
    const payload: IniciarSesionRequest = { codigoAcceso };
    return this.http.post<IniciarSesionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/iniciar`,
      payload
    );
  }

  registrarRespuesta(
    sesionId: string,
    payload: {
      readonly preguntaId: string;
      readonly texto: string;
      readonly tiempoEmpleadoSegundos: number;
      readonly fueExpirado: boolean;
    }
  ) {
    const body: RegistrarRespuestaRequest = payload;
    return this.http.post<RegistrarRespuestaResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/respuestas`,
      body
    );
  }

  registrarViolacionPestana(sesionId: string) {
    return this.http.post(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/violaciones-pestana`, {});
  }

  subirAdjunto(sesionId: string, preguntaId: string, archivo: File) {
    const formData = new FormData();
    formData.append('preguntaId', preguntaId);
    formData.append('archivo', archivo, archivo.name);
    return this.http.post<{ urlAdjunto: string }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/adjuntos`,
      formData
    );
  }

  listarSesionesCandidato(pagina = 1, tamanoPagina = 10, estado?: string, busqueda?: string) {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanoPagina', tamanoPagina);
    if (estado) params = params.set('estado', estado);
    if (busqueda) params = params.set('busqueda', busqueda);
    return this.http.get<ResultadoPaginado<SesionCandidatoDto>>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/candidato`, { params }
    );
  }

  crearSesionesMasivas(payload: CrearSesionesMasivasRequest) {
    return this.http.post<CrearSesionesMasivasResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/masivas`, payload
    );
  }

  subirGrabacion(sesionId: string, blob: Blob) {
    const formData = new FormData();
    formData.append('archivo', blob, 'grabacion.webm');
    return this.http.post<{ urlGrabacion: string }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/grabacion`,
      formData
    );
  }

  subirAudio(sesionId: string, blob: Blob) {
    const formData = new FormData();
    formData.append('archivo', blob, 'audio.webm');
    return this.http.post<{ urlGrabacion: string }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.sesiones}/${sesionId}/audio`,
      formData
    );
  }
}
