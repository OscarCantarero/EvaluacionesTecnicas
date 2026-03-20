using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Sesiones.Events;

/// <summary>
/// Se dispara cuando una sesión de evaluación es iniciada
/// </summary>
public sealed record SesionIniciadaEvent(
    Guid SesionId,
    Guid EvaluacionId,
    string CandidatoId,
    DateTime IniciadoEn
) : IDomainEvent;

/// <summary>
/// Se dispara cuando un candidato responde una pregunta
/// </summary>
public sealed record RespuestaRegistradaEvent(
    Guid SesionId,
    Guid PreguntaId,
    string TextoRespuesta,
    int TiempoEmpleadoSegundos,
    DateTime BrindadaEn
) : IDomainEvent;

/// <summary>
/// Se dispara cuando se detecta que el candidato cambió de pestaña
/// </summary>
public sealed record ViolacionPestanaDetectadaEvent(
    Guid SesionId,
    DateTime DetectadoEn,
    int ContadorViolaciones
) : IDomainEvent;

/// <summary>
/// Se dispara cuando la sesión se completa (todas las preguntas respondidas o tiempo agotado)
/// </summary>
public sealed record SesionCompletadaEvent(
    Guid SesionId,
    DateTime CompletadoEn,
    int PreguntasRespondidas,
    int TiempoTotalSegundos
) : IDomainEvent;

/// <summary>
/// Se dispara cuando el tiempo de una pregunta expira
/// </summary>
public sealed record TiempoExpiratoEvent(
    Guid SesionId,
    Guid PreguntaId,
    DateTime ExpiradoEn
) : IDomainEvent;
