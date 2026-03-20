using MediatR;

namespace TechEval.Application.Sesiones.Commands.CrearSesion;

public sealed record CrearSesionCommand(
    Guid EvaluacionId,
    string CandidatoId
) : IRequest<CrearSesionResponse>;

public sealed record CrearSesionResponse(
    Guid SesionId,
    string CodigoAcceso,
    string UrlSesion
);
