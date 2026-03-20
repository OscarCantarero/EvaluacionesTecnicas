using MediatR;

namespace TechEval.Application.Sesiones.Commands.AgregarAdjuntoRespuesta;

public sealed record AgregarAdjuntoRespuestaCommand(
    Guid SesionId,
    Guid PreguntaId,
    string UrlAdjunto
) : IRequest;
