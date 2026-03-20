using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.EliminarOpcion;

public sealed record EliminarOpcionCommand(Guid EvaluacionId, Guid PreguntaId, Guid OpcionId) : IRequest;

public sealed class EliminarOpcionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<EliminarOpcionCommand>
{
    public async Task Handle(EliminarOpcionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        evaluacion.EliminarOpcion(command.PreguntaId, command.OpcionId);
        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
