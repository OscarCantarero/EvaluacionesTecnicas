namespace TechEval.Application.Common.Interfaces;

/// <summary>
/// Servicio de evaluación automática con IA
/// Anti-Corruption Layer: Abstrae el endpoint externo de IA
/// </summary>
public interface IServicioEvaluacionIA
{
    /// <summary>
    /// Evalúa una respuesta libre usando el endpoint de IA externo
    /// </summary>
    Task<RespuestaEvaluacionIA> EvaluarRespuestaAsync(
        SolicitudEvaluacionIA solicitud,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Solicitud estructurada para el servicio de IA
/// </summary>
public sealed record SolicitudEvaluacionIA(
    Guid PreguntaId,
    string TextoPregunta,
    string RespuestaCandidato,
    string ContextoEvaluador,
    int EscalaPuntuacion,
    string? Rubrica
);

/// <summary>
/// Respuesta del servicio de IA adaptada al dominio
/// </summary>
public sealed record RespuestaEvaluacionIA(
    Guid PreguntaId,
    decimal PuntajeSugerido,
    int EscalaPuntuacion,
    string Justificacion,
    bool RequiereRevisionManual,
    List<string> AspectosPositivos,
    List<string> AspectosNegativos
);
