using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.EliminarPregunta;

public sealed record EliminarPreguntaCommand(Guid EvaluacionId, Guid PreguntaId) : IRequest;

public sealed class EliminarPreguntaCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<EliminarPreguntaCommand>
{
    public async Task Handle(EliminarPreguntaCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        evaluacion.EliminarPregunta(command.PreguntaId);
        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
