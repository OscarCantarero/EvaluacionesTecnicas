using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.RegistrarViolacionPestana;

public sealed class RegistrarViolacionPestanaCommandHandler : IRequestHandler<RegistrarViolacionPestanaCommand>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IContextoUsuario _contextoUsuario;

    public RegistrarViolacionPestanaCommandHandler(
        IRepositorioSesionEvaluacion repo,
        IContextoUsuario contextoUsuario)
    {
        _repo = repo;
        _contextoUsuario = contextoUsuario;
    }

    public async Task Handle(
        RegistrarViolacionPestanaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerPorIdAsync(command.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Sesiones.SesionEvaluacion), command.SesionId);

        if (sesion.CandidatoId != _contextoUsuario.UsuarioId)
            throw new UnauthorizedException("No tienes acceso a esta sesión");

        sesion.RegistrarViolacionPestana();
        await _repo.ActualizarAsync(sesion, cancellationToken);
    }
}
