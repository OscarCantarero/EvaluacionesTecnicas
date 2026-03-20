using Microsoft.Extensions.Logging;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Infrastructure.Services;

/// <summary>
/// Implementación placeholder del servicio de IA.
/// Genera una evaluación simulada que respeta la escala y el contexto del evaluador.
/// Será reemplazada por la integración real con el endpoint externo.
/// </summary>
public sealed class ServicioEvaluacionIAPlaceholder : IServicioEvaluacionIA
{
    private readonly ILogger<ServicioEvaluacionIAPlaceholder> _logger;

    public ServicioEvaluacionIAPlaceholder(ILogger<ServicioEvaluacionIAPlaceholder> logger)
    {
        _logger = logger;
    }

    public async Task<RespuestaEvaluacionIA> EvaluarRespuestaAsync(
        SolicitudEvaluacionIA solicitud,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Evaluación de IA solicitada para pregunta {PreguntaId} con escala {Escala} y contexto '{Contexto}' (PLACEHOLDER)",
            solicitud.PreguntaId,
            solicitud.EscalaPuntuacion,
            solicitud.ContextoEvaluador
        );

        // PLACEHOLDER: Simular respuesta estructurada
        // TODO: Reemplazar con llamada HTTP real al endpoint de IA
        var puntajePlaceholder = Math.Round(solicitud.EscalaPuntuacion * 0.7m, 1);

        var respuestaIA = new RespuestaEvaluacionIA(
            PreguntaId: solicitud.PreguntaId,
            PuntajeSugerido: puntajePlaceholder,
            EscalaPuntuacion: solicitud.EscalaPuntuacion,
            Justificacion: $"[PLACEHOLDER] Evaluación simulada como '{solicitud.ContextoEvaluador}'. "
                + $"Puntaje {puntajePlaceholder}/{solicitud.EscalaPuntuacion}. "
                + "Cuando se conecte el endpoint real de IA, esta respuesta será reemplazada por una evaluación genuina.",
            RequiereRevisionManual: true,
            AspectosPositivos: new List<string>
            {
                "[PLACEHOLDER] La respuesta fue proporcionada dentro del tiempo."
            },
            AspectosNegativos: new List<string>
            {
                "[PLACEHOLDER] No es posible evaluar el contenido real sin el servicio de IA conectado."
            }
        );

        _logger.LogInformation(
            "Respuesta de IA generada para pregunta {PreguntaId}: {Puntaje}/{Escala}",
            solicitud.PreguntaId,
            respuestaIA.PuntajeSugerido,
            respuestaIA.EscalaPuntuacion
        );

        return await Task.FromResult(respuestaIA);
    }
}
