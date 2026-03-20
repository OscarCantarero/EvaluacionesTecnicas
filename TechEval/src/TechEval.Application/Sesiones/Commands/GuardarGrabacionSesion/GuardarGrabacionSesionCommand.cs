using MediatR;

namespace TechEval.Application.Sesiones.Commands.GuardarGrabacionSesion;

public sealed record GuardarGrabacionSesionCommand(
    Guid SesionId,
    string UrlGrabacion
) : IRequest;
