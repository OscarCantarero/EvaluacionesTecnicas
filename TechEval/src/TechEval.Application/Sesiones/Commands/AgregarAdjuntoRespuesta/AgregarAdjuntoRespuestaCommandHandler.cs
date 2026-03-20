using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.AgregarAdjuntoRespuesta;

public sealed class AgregarAdjuntoRespuestaCommandHandler : IRequestHandler<AgregarAdjuntoRespuestaCommand>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IContextoUsuario _contextoUsuario;

    public AgregarAdjuntoRespuestaCommandHandler(
        IRepositorioSesionEvaluacion repo,
        IContextoUsuario contextoUsuario)
    {
        _repo = repo;
        _contextoUsuario = contextoUsuario;
    }

    public async Task Handle(
        AgregarAdjuntoRespuestaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerConPreguntasAsync(command.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Sesiones.SesionEvaluacion), command.SesionId);

        if (sesion.CandidatoId != _contextoUsuario.UsuarioId)
            throw new UnauthorizedException("No tienes acceso a esta sesión");

        sesion.AgregarAdjuntoARespuesta(command.PreguntaId, command.UrlAdjunto);
        await _repo.ActualizarAsync(sesion, cancellationToken);
    }
}
