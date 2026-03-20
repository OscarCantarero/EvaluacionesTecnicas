using MediatR;
using TechEval.Domain.Resultados;

namespace TechEval.Application.Resultados.Commands.SubirTranscripcion;

public sealed record SubirTranscripcionCommand(
    Guid SesionId,
    TipoTranscripcion Tipo,
    string? Contenido = null,
    string? UrlArchivo = null
) : IRequest<SubirTranscripcionResponse>;

public sealed record SubirTranscripcionResponse(
    Guid TranscripcionId,
    string Estado,
    string Tipo
);
