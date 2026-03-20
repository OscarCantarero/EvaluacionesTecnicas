using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.GuardarGrabacionSesion;

public sealed class GuardarGrabacionSesionCommandHandler : IRequestHandler<GuardarGrabacionSesionCommand>
{
    private readonly IRepositorioSesionEvaluacion _repo;

    public GuardarGrabacionSesionCommandHandler(IRepositorioSesionEvaluacion repo)
    {
        _repo = repo;
    }

    public async Task Handle(
        GuardarGrabacionSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerPorIdAsync(command.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Sesiones.SesionEvaluacion), command.SesionId);

        sesion.GuardarGrabacionSesion(command.UrlGrabacion);
        await _repo.ActualizarAsync(sesion, cancellationToken);
    }
}
