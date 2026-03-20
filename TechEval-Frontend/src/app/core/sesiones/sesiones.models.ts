export interface OpcionPreguntaDto {
  readonly id: string;
  readonly texto: string;
}

export interface PreguntaActualDto {
  readonly preguntaId: string;
  readonly numero: number;
  readonly tipo: string;
  readonly texto: string;
  readonly dificultad: string;
  readonly tiempoMaximoSegundos: number | null;
  readonly opciones: ReadonlyArray<OpcionPreguntaDto>;
  readonly permiteAdjunto: boolean;
}

// Shape real que devuelve el backend (PreguntaSesionDto en C#)
export interface PreguntaSesionBackendDto {
  readonly preguntaId: string;
  readonly texto: string;
  readonly orden: number;
  readonly limiteTiempoSegundos: number | null;
  readonly tipoPregunta: string;  // 'TextoLibre' | 'SeleccionUnica' | 'SeleccionMultiple'
  readonly opciones: ReadonlyArray<OpcionPreguntaDto>;
}

export interface IniciarSesionResponse {
  readonly sesionId: string;
  readonly preguntaActual: PreguntaSesionBackendDto | null;
  readonly totalPreguntas: number;
  readonly preguntasRespondidas: number;
}

export interface RegistrarRespuestaResponse {
  readonly sesionId: string;
  readonly sesionCompletada: boolean;
  readonly preguntaSiguiente: PreguntaSesionBackendDto | null;
  readonly preguntasRespondidas: number;
  readonly totalPreguntas: number;
}

export interface SesionCandidatoDto {
  readonly sesionId: string;
  readonly evaluacionTitulo: string;
  readonly estado: string;
  readonly fechaCreacion: string;
  readonly fechaFin: string | null;
  readonly puntuacionObtenida: number | null;
}
