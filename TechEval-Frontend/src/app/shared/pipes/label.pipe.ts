import { Pipe, PipeTransform } from '@angular/core';

const LABELS: Record<string, string> = {
  // TipoPregunta
  TextoLibre: 'Texto libre',
  SeleccionUnica: 'Selección única',
  SeleccionMultiple: 'Selección múltiple',

  // NivelDificultad
  Facil: 'Fácil',
  Medio: 'Medio',
  Dificil: 'Difícil',

  // EstadoSesion
  NoIniciada: 'No iniciada',
  EnProgreso: 'En progreso',
  Completada: 'Completada',
  Cancelada: 'Cancelada',

  // EstadoEvaluacion
  Borrador: 'Borrador',
  Activa: 'Activa',
  Cerrada: 'Cerrada',
  Archivada: 'Archivada',

  // EstadoRevision
  PendienteRevision: 'Pendiente de revisión',
  Pendiente: 'Pendiente',
  EnRevision: 'En revisión',

  // EstadoGeneral resultado (nuevo: Aprobado/No aprobado)
  Aprobado: 'Aprobado',
  'No aprobado': 'No aprobado',

  // EstadoGeneral resultado (legacy)
  Excelente: 'Excelente',
  'Muy Bueno': 'Muy bueno',
  Bueno: 'Bueno',
  Aceptable: 'Aceptable',
  Insuficiente: 'Insuficiente',

  // EstadoTranscripcion
  Subida: 'Subida',
  EvaluadaPorIA: 'Evaluada por IA',

  // TipoTranscripcion
  Sesion: 'Sesión',
  Entrevista: 'Entrevista'
};

@Pipe({ name: 'label', pure: true })
export class LabelPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '';
    return LABELS[value] ?? value;
  }
}
