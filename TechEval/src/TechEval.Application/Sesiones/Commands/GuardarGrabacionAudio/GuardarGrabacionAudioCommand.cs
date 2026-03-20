using MediatR;

namespace TechEval.Application.Sesiones.Commands.GuardarGrabacionAudio;

public sealed record GuardarGrabacionAudioCommand(
    Guid SesionId,
    string UrlGrabacion
) : IRequest;
