using MediatR;

namespace TechEval.Application.Resultados.Commands.EvaluarTranscripcionConIA;

public sealed record EvaluarTranscripcionConIACommand(
    Guid SesionId,
    Guid TranscripcionId,
    string ContextoEvaluador
) : IRequest<EvaluarTranscripcionConIAResponse>;

public sealed record EvaluarTranscripcionConIAResponse(
    Guid TranscripcionId,
    decimal PuntajeIA,
    string Justificacion,
    decimal NuevoPorcentajeObtenido
);
