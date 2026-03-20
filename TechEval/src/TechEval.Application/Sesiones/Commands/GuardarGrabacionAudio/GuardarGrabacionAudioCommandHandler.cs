using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.GuardarGrabacionAudio;

public sealed class GuardarGrabacionAudioCommandHandler : IRequestHandler<GuardarGrabacionAudioCommand>
{
    private readonly IRepositorioSesionEvaluacion _repo;

    public GuardarGrabacionAudioCommandHandler(IRepositorioSesionEvaluacion repo)
    {
        _repo = repo;
    }

    public async Task Handle(
        GuardarGrabacionAudioCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerPorIdAsync(command.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Sesiones.SesionEvaluacion), command.SesionId);

        sesion.GuardarGrabacionAudio(command.UrlGrabacion);
        await _repo.ActualizarAsync(sesion, cancellationToken);
    }
}
