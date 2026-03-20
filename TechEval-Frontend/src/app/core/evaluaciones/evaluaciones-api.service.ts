import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CONFIG } from '../config/api-config';
import { ResultadoPaginado } from '../common/pagination.models';
import {
  ActualizarEvaluacionRequest,
  ActualizarOpcionRequest,
  ActualizarPreguntaRequest,
  CompararCandidatosRequest,
  CompararCandidatosResponse,
  CrearEvaluacionRequest,
  CrearEvaluacionResponse,
  CrearOpcionRequest,
  CrearOpcionResponse,
  CrearPreguntaRequest,
  CrearPreguntaResponse,
  EvaluacionDetalleDto,
  EvaluacionResumenDto,
  ObtenerRankingResponse,
  PreguntaBancoDto
} from './evaluaciones.models';

@Injectable({ providedIn: 'root' })
export class EvaluacionesApiService {
  private readonly http = inject(HttpClient);

  listarEvaluaciones(soloMias = false, pagina = 1, tamanoPagina = 10, busqueda?: string, estado?: string) {
    let params = new HttpParams()
      .set('soloMias', soloMias)
      .set('pagina', pagina)
      .set('tamanoPagina', tamanoPagina);
    if (busqueda) params = params.set('busqueda', busqueda);
    if (estado) params = params.set('estado', estado);
    return this.http.get<ResultadoPaginado<EvaluacionResumenDto>>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}`, { params }
    );
  }

  obtenerEvaluacion(evaluacionId: string) {
    return this.http.get<EvaluacionDetalleDto>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}`);
  }

  crearEvaluacion(payload: CrearEvaluacionRequest) {
    return this.http.post<CrearEvaluacionResponse>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}`, payload);
  }

  actualizarEvaluacion(evaluacionId: string, payload: ActualizarEvaluacionRequest) {
    return this.http.put<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}`, payload);
  }

  activarEvaluacion(evaluacionId: string) {
    return this.http.put<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/activar`, {});
  }

  eliminarEvaluacion(evaluacionId: string) {
    return this.http.delete<void>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}`);
  }

  crearPregunta(evaluacionId: string, payload: CrearPreguntaRequest) {
    return this.http.post<CrearPreguntaResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas`,
      payload
    );
  }

  actualizarPregunta(evaluacionId: string, preguntaId: string, payload: ActualizarPreguntaRequest) {
    return this.http.put<void>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/${preguntaId}`,
      payload
    );
  }

  eliminarPregunta(evaluacionId: string, preguntaId: string) {
    return this.http.delete<void>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/${preguntaId}`
    );
  }

  crearOpcion(evaluacionId: string, preguntaId: string, payload: CrearOpcionRequest) {
    return this.http.post<CrearOpcionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/${preguntaId}/opciones`,
      payload
    );
  }

  actualizarOpcion(evaluacionId: string, preguntaId: string, opcionId: string, payload: ActualizarOpcionRequest) {
    return this.http.put<void>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/${preguntaId}/opciones/${opcionId}`,
      payload
    );
  }

  eliminarOpcion(evaluacionId: string, preguntaId: string, opcionId: string) {
    return this.http.delete<void>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/${preguntaId}/opciones/${opcionId}`
    );
  }

  compararCandidatos(evaluacionId: string, payload: CompararCandidatosRequest) {
    return this.http.post<CompararCandidatosResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/comparar`,
      payload
    );
  }

  obtenerRanking(evaluacionId: string) {
    return this.http.get<ObtenerRankingResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/ranking`
    );
  }

  obtenerPreguntasBanco(evaluacionId: string, categoriaIds?: string, dificultades?: string, tipoPregunta?: string) {
    let params = new HttpParams();
    if (categoriaIds) params = params.set('categoriaIds', categoriaIds);
    if (dificultades) params = params.set('dificultades', dificultades);
    if (tipoPregunta) params = params.set('tipoPregunta', tipoPregunta);
    return this.http.get<PreguntaBancoDto[]>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/banco`,
      { params }
    );
  }

  agregarDelBanco(evaluacionId: string, preguntaIds: string[]) {
    return this.http.post<{ preguntasAgregadas: number; nuevosIds: string[] }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.evaluaciones}/${evaluacionId}/preguntas/agregar-del-banco`,
      { preguntaIds }
    );
  }
}
