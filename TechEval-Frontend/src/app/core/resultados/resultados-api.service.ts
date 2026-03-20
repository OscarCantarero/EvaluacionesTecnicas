import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CONFIG } from '../config/api-config';

export interface AsignarPuntuacionRequest {
  readonly preguntaId: string;
  readonly puntaje: number;
  readonly observaciones: string | null;
}

export interface AsignarPuntuacionResponse {
  readonly exitoAsignacion: boolean;
  readonly puntajeTotal: number;
  readonly porcentajeObtenido: number;
  readonly mensaje: string;
}

export interface GenerarPdfResponse {
  readonly exitoGeneration: boolean;
  readonly urlPDF: string;
  readonly mensaje: string;
}

export interface EvaluarIaRequest {
  readonly preguntaId: string;
  readonly contextoEvaluador: string;
  readonly escalaPuntuacion: number;
  readonly rubrica: string | null;
}

export interface EvaluarIaResponse {
  readonly preguntaId: string;
  readonly puntajeSugerido: number;
  readonly escalaPuntuacion: number;
  readonly justificacion: string;
  readonly requiereRevisionManual: boolean;
  readonly aspectosPositivos: ReadonlyArray<string>;
  readonly aspectosNegativos: ReadonlyArray<string>;
}

export interface DecisionIARequest {
  readonly preguntaId: string;
  readonly puntajeAceptado?: number | null;
  readonly puntajeManualAlternativo?: number | null;
  readonly motivoRechazo?: string | null;
}

export interface ResultadoPuntuacionDto {
  readonly numeroPregunta: number;
  readonly tipoPregunta: string;
  readonly respuesta: string;
  readonly puntuacionAutomatica: number | null;
  readonly puntuacionManual: number | null;
  readonly puntuacionIASugerida: number | null;
  readonly justificacionIA: string | null;
  readonly observaciones: string | null;
  readonly fueExpirada: boolean;
  readonly urlAdjunto: string | null;
  readonly tiempoEmpleadoSegundos: number;
  readonly preguntaId: string;
}

export interface ResultadoSesionDto {
  readonly resultadoId: string;
  readonly sesionId: string;
  readonly nombreCandidato: string;
  readonly tituloEvaluacion: string;
  readonly puntuacionTotal: number;
  readonly puntuacionMaxima: number;
  readonly porcentajeObtenido: number;
  readonly estadoGeneral: string;
  readonly violacionesPestana: number;
  readonly tiempoTotalSegundos: number;
  readonly estadoRevision: string;
  readonly puntuaciones: ReadonlyArray<ResultadoPuntuacionDto>;
  readonly transcripciones: ReadonlyArray<TranscripcionDto>;
}

export interface TranscripcionDto {
  readonly id: string;
  readonly tipo: string;
  readonly contenido: string | null;
  readonly urlArchivo: string | null;
  readonly puntajeIA: number | null;
  readonly justificacionIA: string | null;
  readonly estado: string;
}

export interface CompletarRevisionResponse {
  readonly exito: boolean;
  readonly estadoRevision: string;
  readonly mensaje: string;
  readonly emailEnviado: boolean;
}

@Injectable({ providedIn: 'root' })
export class ResultadosApiService {
  private readonly http = inject(HttpClient);

  obtenerResultado(sesionId: string) {
    return this.http.get<ResultadoSesionDto>(`${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}`);
  }

  asignarPuntuacion(sesionId: string, payload: AsignarPuntuacionRequest) {
    return this.http.patch<AsignarPuntuacionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/puntuaciones`,
      payload
    );
  }

  generarPdf(sesionId: string) {
    return this.http.post<GenerarPdfResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/pdf`,
      {}
    );
  }

  evaluarConIa(sesionId: string, payload: EvaluarIaRequest) {
    return this.http.post<EvaluarIaResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/ia-evaluate`,
      payload
    );
  }

  aceptarSugerenciaIa(sesionId: string, payload: DecisionIARequest) {
    return this.http.post<AsignarPuntuacionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/ia-aceptar`,
      payload
    );
  }

  rechazarSugerenciaIa(sesionId: string, payload: DecisionIARequest) {
    return this.http.post<AsignarPuntuacionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/ia-rechazar`,
      payload
    );
  }

  completarRevision(sesionId: string) {
    return this.http.post<CompletarRevisionResponse>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/completar-revision`,
      {}
    );
  }

  subirTranscripcion(sesionId: string, tipo: string, contenido: string | null, archivo: File | null) {
    const formData = new FormData();
    formData.append('tipo', tipo);
    if (contenido) formData.append('contenido', contenido);
    if (archivo) formData.append('archivo', archivo, archivo.name);
    return this.http.post<{ transcripcionId: string; estado: string; tipo: string }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/transcripciones`,
      formData
    );
  }

  evaluarTranscripcionConIa(sesionId: string, transcripcionId: string, contextoEvaluador: string) {
    return this.http.post<{ transcripcionId: string; puntajeIA: number; justificacion: string; nuevoPorcentajeObtenido: number }>(
      `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.resultados}/${sesionId}/transcripciones/${transcripcionId}/evaluar-ia`,
      { contextoEvaluador }
    );
  }
}
