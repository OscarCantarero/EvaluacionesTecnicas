export type TipoPregunta = 'TextoLibre' | 'SeleccionUnica' | 'SeleccionMultiple';
export type NivelDificultad = 'Facil' | 'Medio' | 'Dificil';

export interface EvaluacionResumenDto {
  readonly id: string;
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly estado: string;
  readonly ordenAleatorio: boolean;
  readonly ordenPorDificultad: boolean;
  readonly totalPreguntas: number;
  readonly creadoEn: string;
}

export interface OpcionRespuestaDto {
  readonly id: string;
  readonly texto: string;
  readonly puntuacion: number | null;
  readonly esRevisionManual: boolean;
  readonly orden: number;
}

export interface PreguntaDto {
  readonly id: string;
  readonly texto: string;
  readonly tipoPregunta: TipoPregunta;
  readonly nivelDificultad: NivelDificultad;
  readonly limiteTiempoSegundos: number | null;
  readonly permiteAdjunto: boolean;
  readonly esRevisionManual: boolean;
  readonly orden: number;
  readonly opciones: ReadonlyArray<OpcionRespuestaDto>;
}

export interface EvaluacionDetalleDto {
  readonly id: string;
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly estado: string;
  readonly ordenAleatorio: boolean;
  readonly ordenPorDificultad: boolean;
  readonly creadoPor: string;
  readonly creadoEn: string;
  readonly actualizadoEn: string | null;
  readonly preguntas: ReadonlyArray<PreguntaDto>;
}

export interface CrearEvaluacionRequest {
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly ordenAleatorio: boolean;
  readonly ordenPorDificultad: boolean;
}

export interface ActualizarEvaluacionRequest {
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly ordenAleatorio: boolean;
  readonly ordenPorDificultad: boolean;
}

export interface CrearEvaluacionResponse {
  readonly id: string;
}

export interface CrearPreguntaRequest {
  readonly texto: string;
  readonly tipoPregunta: TipoPregunta;
  readonly nivelDificultad: NivelDificultad;
  readonly limiteTiempoSegundos: number | null;
  readonly permiteAdjunto: boolean;
  readonly esRevisionManual: boolean;
}

export interface ActualizarPreguntaRequest {
  readonly texto: string;
  readonly tipoPregunta: TipoPregunta;
  readonly nivelDificultad: NivelDificultad;
  readonly limiteTiempoSegundos: number | null;
  readonly permiteAdjunto: boolean;
  readonly esRevisionManual: boolean;
}

export interface CrearPreguntaResponse {
  readonly id: string;
}

export interface CrearOpcionRequest {
  readonly texto: string;
  readonly puntuacion: number | null;
  readonly esRevisionManual: boolean;
}

export interface ActualizarOpcionRequest {
  readonly texto: string;
  readonly puntuacion: number | null;
  readonly esRevisionManual: boolean;
}

export interface CrearOpcionResponse {
  readonly id: string;
}

// ── Comparación y Ranking ──

export interface CompararCandidatosRequest {
  readonly sesionIds: string[];
}

export interface CandidatoComparativoDto {
  readonly sesionId: string;
  readonly candidatoId: string;
  readonly nombreCandidato: string;
  readonly puntuacionTotal: number;
  readonly porcentajeObtenido: number;
  readonly estadoGeneral: string;
  readonly violacionesPestana: number;
  readonly tiempoTotalSegundos: number;
  readonly estadoRevision: string;
}

export interface RespuestaComparativaDto {
  readonly sesionId: string;
  readonly nombreCandidato: string;
  readonly respuesta: string;
  readonly puntuacionFinal: number;
  readonly fueExpirada: boolean;
}

export interface PreguntaComparativaDto {
  readonly numeroPregunta: number;
  readonly tipoPregunta: string;
  readonly respuestas: ReadonlyArray<RespuestaComparativaDto>;
}

export interface CompararCandidatosResponse {
  readonly evaluacionId: string;
  readonly tituloEvaluacion: string;
  readonly candidatos: ReadonlyArray<CandidatoComparativoDto>;
  readonly preguntas: ReadonlyArray<PreguntaComparativaDto>;
}

export interface CandidatoRankingDto {
  readonly posicion: number;
  readonly sesionId: string;
  readonly candidatoId: string;
  readonly nombreCandidato: string;
  readonly puntuacionTotal: number;
  readonly porcentajeObtenido: number;
  readonly estadoGeneral: string;
  readonly violacionesPestana: number;
  readonly tiempoTotalSegundos: number;
  readonly estadoRevision: string;
  readonly fechaCompletacion: string;
}

export interface ObtenerRankingResponse {
  readonly evaluacionId: string;
  readonly tituloEvaluacion: string;
  readonly totalCandidatos: number;
  readonly ranking: ReadonlyArray<CandidatoRankingDto>;
}
