using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarOpcion;

public sealed record ActualizarOpcionCommand(
    Guid EvaluacionId,
    Guid PreguntaId,
    Guid OpcionId,
    string Texto,
    int? Puntuacion,
    bool EsRevisionManual
) : IRequest;

public sealed class ActualizarOpcionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<ActualizarOpcionCommand>
{
    public async Task Handle(ActualizarOpcionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        evaluacion.ActualizarOpcion(command.PreguntaId, command.OpcionId, command.Texto, command.Puntuacion, command.EsRevisionManual);
        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
