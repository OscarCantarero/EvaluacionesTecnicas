using MediatR;
using TechEval.Application.Common.DTOs;

namespace TechEval.Application.Sesiones.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(
    string CodigoAcceso
) : IRequest<IniciarSesionResponse>;

public sealed record IniciarSesionResponse(
    Guid SesionId,
    PreguntaSesionDto? PreguntaActual,
    int TotalPreguntas,
    int PreguntasRespondidas
);
