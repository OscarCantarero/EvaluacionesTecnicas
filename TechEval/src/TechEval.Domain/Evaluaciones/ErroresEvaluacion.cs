using TechEval.Domain.Common.Errors;

namespace TechEval.Domain.Evaluaciones;

public static class ErroresEvaluacion
{
    public static readonly ErrorDominio NoEncontrada = new(
        "Evaluacion.NoEncontrada", "La evaluación no fue encontrada.");

    public static readonly ErrorDominio OrdenConflicto = new(
        "Evaluacion.OrdenConflicto", "No se puede activar orden aleatorio y progresivo al mismo tiempo.");

    public static readonly ErrorDominio TieneSessionesActivas = new(
        "Evaluacion.TieneSessionesActivas", "No se puede modificar la evaluación porque tiene sesiones activas.");

    public static readonly ErrorDominio PreguntaNoEncontrada = new(
        "Evaluacion.PreguntaNoEncontrada", "La pregunta no pertenece a esta evaluación.");

    public static readonly ErrorDominio OpcionNoEncontrada = new(
        "Evaluacion.OpcionNoEncontrada", "La opción de respuesta no fue encontrada.");

    public static readonly ErrorDominio OpcionEnPreguntaLibre = new(
        "Evaluacion.OpcionEnPreguntaLibre", "Las preguntas de texto libre no pueden tener opciones de respuesta.");

    public static readonly ErrorDominio PuntuacionConRevisionManual = new(
        "Evaluacion.PuntuacionConRevisionManual", "No se puede asignar puntaje automático y revisión manual a la misma opción.");

    public static readonly ErrorDominio LimiteTiempoInvalido = new(
        "Evaluacion.LimiteTiempoInvalido", "El límite de tiempo debe ser mayor a cero.");

    public static readonly ErrorDominio CantidadPreguntasInvalida = new(
        "Evaluacion.CantidadPreguntasInvalida",
        "La cantidad de preguntas por sesión debe ser mayor que 0.");

    public static readonly ErrorDominio DistribucionRequerida = new(
        "Evaluacion.DistribucionRequerida",
        "Se requiere una distribución de dificultad cuando el modo es PorDistribucionDificultad.");
}
